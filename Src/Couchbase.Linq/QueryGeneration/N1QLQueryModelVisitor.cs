﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Annotations;
using Couchbase.Core.Serialization;
using Couchbase.Linq.Clauses;
using Couchbase.Linq.Execution;
using Couchbase.Linq.Operators;
using Couchbase.Linq.QueryGeneration.ExpressionTransformers;
using Couchbase.Linq.QueryGeneration.MemberNameResolvers;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.ExpressionVisitors;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;

namespace Couchbase.Linq.QueryGeneration
{
    internal class N1QlQueryModelVisitor : QueryModelVisitorBase, IN1QlQueryModelVisitor
    {
        #region Constants

        private enum VisitStatus
        {
            None,

            // Group subqueries are used when clauses are applied after a group operation, such as .Where clauses
            // which are treated as HAVING statements.
            InGroupSubquery,
            AfterGroupSubquery,

            // Union sort subquerys are used if .OrderBy clauses are applied after performing a union
            // The require special handling because the sorting is now on the generic name of the columns being returned
            // Not on the original column names
            InUnionSortSubquery,
            AfterUnionSortSubquery
        }

        #endregion

        private readonly N1QlQueryGenerationContext _queryGenerationContext;
        private readonly QueryPartsAggregator _queryPartsAggregator = new QueryPartsAggregator();
        private readonly List<UnclaimedGroupJoin> _unclaimedGroupJoins = new List<UnclaimedGroupJoin>();
        private readonly ScalarResultBehavior _scalarResultBehavior = new ScalarResultBehavior();

        private readonly bool _isSubQuery = false;

        /// <summary>
        /// Indicates if an aggregate has been applied, which may change select clause handling
        /// </summary>
        private bool _isAggregated = false;

        /// <summary>
        /// Tracks special status related to the visiting process, which may alter the behavior as query model
        /// clauses are being visited.  For example, .Where clauses are treating as HAVING statements if
        /// _visitStatus == AfterGroupSubquery.
        /// </summary>
        private VisitStatus _visitStatus = VisitStatus.None;

        /// <summary>
        /// Stores the mappings between expressions outside the group query to the extents inside
        /// </summary>
        private ExpressionTransformerRegistry _groupingExpressionTransformerRegistry;

        /// <summary>
        /// Provides information about how scalar results should be extracted from the N1QL query result after execution.
        /// </summary>
        public ScalarResultBehavior ScalarResultBehavior
        {
            get { return _scalarResultBehavior; }
        }

        public N1QlQueryModelVisitor(IMemberNameResolver memberNameResolver, IMethodCallTranslatorProvider methodCallTranslatorProvider,
            ITypeSerializer serializer)
        {
            _queryGenerationContext = new N1QlQueryGenerationContext()
            {
                //MemberNameResolver = new JsonNetMemberNameResolver(ClusterHelper.Get().Configuration.SerializationSettings.ContractResolver),
                //MethodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider()
                MemberNameResolver = memberNameResolver,
                MethodCallTranslatorProvider = methodCallTranslatorProvider,
                Serializer = serializer
            };
        }

        public N1QlQueryModelVisitor(N1QlQueryGenerationContext queryGenerationContext) : this(queryGenerationContext, false)
        {
        }

        /// <exception cref="ArgumentNullException"><paramref name="queryGenerationContext"/> is <see langword="null" />.</exception>
        public N1QlQueryModelVisitor(N1QlQueryGenerationContext queryGenerationContext, bool isSubQuery)
        {
            if (queryGenerationContext == null)
            {
                throw new ArgumentNullException("queryGenerationContext");
            }

            _queryGenerationContext = queryGenerationContext;
            _isSubQuery = isSubQuery;

            if (isSubQuery)
            {
                _queryPartsAggregator.QueryType = N1QlQueryType.Subquery;
            }
        }

        public string GetQuery()
        {
            return _queryPartsAggregator.BuildN1QlQuery();
        }

        /// <exception cref="NotSupportedException">N1QL Requires All Group Joins Have A Matching From Clause Subquery</exception>
        public override void VisitQueryModel(QueryModel queryModel)
        {
            queryModel.MainFromClause.Accept(this, queryModel);
            VisitBodyClauses(queryModel.BodyClauses, queryModel);

            if (_unclaimedGroupJoins.Any())
            {
                if (_queryGenerationContext.ClusterVersion < Versioning.FeatureVersions.IndexJoin)
                {
                    throw new NotSupportedException("N1QL Requires All Group Joins Have A Matching From Clause Subquery");
                }

                // See if any of the unclaimed group joins are really NEST operations

                foreach (var join in _unclaimedGroupJoins)
                {
                    IQuerySource querySource;
                    if (!IsKeyMethodCall(join.JoinClause.OuterKeySelector, out querySource))
                    {
                        throw new NotSupportedException(
                            "N1QL Requires All Group Joins Have A Matching From Clause Subquery Or Use N1QlFunctions.Key For The Outer Key Selector");
                    }

                    var fromQueryPart = ParseIndexNestJoinClause(join.JoinClause, join.GroupJoinClause);
                    _queryPartsAggregator.AddFromPart(fromQueryPart);
                }
            }

            VisitResultOperators(queryModel.ResultOperators, queryModel);

            if ((_visitStatus != VisitStatus.InGroupSubquery) && (_visitStatus != VisitStatus.AfterUnionSortSubquery))
            {
                // Select clause should not be visited for grouping subqueries or for the outer query when sorting unions

                // Select clause must be visited after the from clause and body clauses
                // This ensures that any extents are linked before being referenced in the select statement
                // Select clause must be visited after result operations because Any and All operators
                // May change how we handle the select clause

                queryModel.SelectClause.Accept(this, queryModel);
            }

            if (!string.IsNullOrEmpty(_queryPartsAggregator.ExplainPart))
            {
                // We no longer need to extract the result, because now the query is returning an explanation instead

                ScalarResultBehavior.ResultExtractionRequired = false;
            }
        }

        /// <exception cref="NotSupportedException">N1Ql Bucket Subqueries Require A UseKeys Call</exception>
        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            var bucketConstantExpression = fromClause.FromExpression as ConstantExpression;
            if ((bucketConstantExpression != null) &&
                typeof(IBucketQueryable).IsAssignableFrom(bucketConstantExpression.Type))
            {
                if (_isSubQuery && !queryModel.BodyClauses.Any(p => p is UseKeysClause))
                {
                    throw new NotSupportedException("N1Ql Bucket Subqueries Require A UseKeys Call");
                }

                _queryPartsAggregator.AddFromPart(new N1QlFromQueryPart()
                {
                    Source = N1QlHelpers.EscapeIdentifier(((IBucketQueryable) bucketConstantExpression.Value).BucketName),
                    ItemName = GetExtentName(fromClause)
                });
            }
            else if (fromClause.FromExpression.NodeType == ExpressionType.MemberAccess)
            {
                if (!_isSubQuery)
                {
                    throw new NotSupportedException("Member Access In The Main From Clause Is Only Supported In Subqueries");
                }

                _queryPartsAggregator.AddFromPart(new N1QlFromQueryPart()
                {
                    Source = GetN1QlExpression((MemberExpression) fromClause.FromExpression),
                    ItemName = GetExtentName(fromClause)
                });

                // This is an Array type subquery, since we're querying against a member not a bucket
                _queryPartsAggregator.QueryType = N1QlQueryType.Array;
            }
            else if (fromClause.FromExpression is SubQueryExpression)
            {
                VisitSubQueryFromClause(fromClause, (SubQueryExpression) fromClause.FromExpression);
            }
            else if (fromClause.FromExpression is QuerySourceReferenceExpression)
            {
                if (!fromClause.FromExpression.Equals(_queryGenerationContext.GroupingQuerySource))
                {
                    throw new NotSupportedException("From Clauses May Not Reference Any Query Source Other Than The Grouping Subquery");
                }

                // We're performing an aggregate against a group
                _queryPartsAggregator.QueryType = N1QlQueryType.Aggregate;

                // Ensure that we use the same extent name as the grouping
                _queryGenerationContext.ExtentNameProvider.LinkExtents(_queryGenerationContext.GroupingQuerySource.ReferencedQuerySource, fromClause);
            }
            else if (fromClause.FromExpression is ConstantExpression)
            {
                // From clause for this subquery is a constant array

                _queryPartsAggregator.QueryType = N1QlQueryType.Array;

                _queryPartsAggregator.AddFromPart(new N1QlFromQueryPart()
                {
                    Source = GetN1QlExpression(fromClause.FromExpression),
                    ItemName = GetExtentName(fromClause)
                });
            }

            base.VisitMainFromClause(fromClause, queryModel);
        }

        private void VisitSubQueryFromClause(MainFromClause fromClause, SubQueryExpression subQuery)
        {
            if (subQuery.QueryModel.ResultOperators.Any(p => p is GroupResultOperator))
            {
                // We're applying functions like HAVING clauses after grouping

                _visitStatus = VisitStatus.InGroupSubquery;
                _queryGenerationContext.GroupingQuerySource = new QuerySourceReferenceExpression(fromClause);

                VisitQueryModel(subQuery.QueryModel);

                _visitStatus = VisitStatus.AfterGroupSubquery;
            }
            else if (subQuery.QueryModel.ResultOperators.Any(p => p is UnionResultOperator || p is ConcatResultOperator))
            {
                // We're applying ORDER BY clauses after a UNION statement is completed

                _visitStatus = VisitStatus.InUnionSortSubquery;

                VisitQueryModel(subQuery.QueryModel);

                _visitStatus = VisitStatus.AfterUnionSortSubquery;

                // When visiting the order by clauses after a union, member references shouldn't include extent names.
                // Instead, they should reference the name of the columns without an extent qualifier.
                _queryGenerationContext.ExtentNameProvider.SetBlankExtentName(fromClause);
            }
            else
            {
                throw new NotSupportedException("Subqueries In The Main From Clause Are Only Supported For Grouping And Unions");
            }
        }

        public virtual void VisitUseKeysClause(UseKeysClause clause, QueryModel queryModel, int index)
        {
            _queryPartsAggregator.AddUseKeysPart(GetN1QlExpression(clause.Keys));
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            if (_queryPartsAggregator.QueryType == N1QlQueryType.SubqueryAny)
            {
                // For Any type subqueries, the select statement is unused
                // So just put a *

                _queryPartsAggregator.SelectPart = "*";
            }
            else if (_queryPartsAggregator.QueryType == N1QlQueryType.SubqueryAll)
            {
                // For All type subqueries, the select statement should just provide all extents
                // So they can be referenced by the SATISFIES statement
                // Select statement that was defined originally is unused

                _queryPartsAggregator.SelectPart = GetExtentSelectParameters();
            }
            else
            {
                _queryPartsAggregator.SelectPart = GetSelectParameters(selectClause, queryModel);

                base.VisitSelectClause(selectClause, queryModel);
            }
        }

        private string GetSelectParameters(SelectClause selectClause, QueryModel queryModel)
        {
            string expression;

            if (selectClause.Selector.GetType() == typeof (QuerySourceReferenceExpression))
            {
                if (!_isAggregated)
                {
                    expression = GetN1QlExpression(selectClause.Selector);

                    if (_queryPartsAggregator.QueryType != N1QlQueryType.Array)
                    {
                        expression = string.Concat(expression, ".*", SelectDocumentMetadataIfRequired(queryModel));
                    }
                }
                else
                {
                    // for aggregates, just use "*" (i.e. AggregateFunction = "COUNT", expression = "*" results in COUNT(*)"

                    ScalarResultBehavior.ResultExtractionRequired = true;
                    _queryPartsAggregator.PropertyExtractionPart = N1QlHelpers.EscapeIdentifier("result");
                    expression = "*";
                }
            }
            else if ((selectClause.Selector.NodeType == ExpressionType.New) || (selectClause.Selector.NodeType == ExpressionType.MemberInit))
            {
                if (_queryPartsAggregator.QueryType != N1QlQueryType.Array)
                {
                    var selector = selectClause.Selector;

                    if (_visitStatus == VisitStatus.AfterGroupSubquery)
                    {
                        // SELECT clauses must be remapped to refer directly to the extents in the grouping subquery
                        // rather than refering to the output of the grouping subquery

                        selector = TransformingExpressionVisitor.Transform(selector, _groupingExpressionTransformerRegistry);
                    }

                    if (!_isAggregated)
                    {
                        expression = N1QlExpressionTreeVisitor.GetN1QlSelectNewExpression(selector,
                            _queryGenerationContext);
                    }
                    else
                    {
                        ScalarResultBehavior.ResultExtractionRequired = true;
                        _queryPartsAggregator.PropertyExtractionPart = N1QlHelpers.EscapeIdentifier("result");

                        // Don't use special "x as y" syntax inside an aggregate function, just make a new object with {y: x}
                        expression = GetN1QlExpression(selectClause.Selector);
                    }
                }
                else
                {
                    expression = GetN1QlExpression(selectClause.Selector);
                }
            }
            else
            {
                expression = GetN1QlExpression(selectClause.Selector);

                if (_queryPartsAggregator.QueryType == N1QlQueryType.Subquery)
                {
                    // For LINQ, this subquery is expected to return a list of the specific property being selected
                    // But N1QL will always return a list of objects with a single property
                    // So we need to use an ARRAY statement to convert the list

                    _queryPartsAggregator.PropertyExtractionPart = N1QlHelpers.EscapeIdentifier("result");

                    expression += " as " + _queryPartsAggregator.PropertyExtractionPart;
                }
                else
                {
                    // This is a select expression on the main query that doesn't use a "new" clause, and isn't against an extent
                    // So it will return an array of objects with properties, while LINQ is expecting an array of values
                    // Since we can't extract the properties from the object in N1QL like we can with subqueries
                    // We need to indicate to the BucketQueryExecutor that it will need to extract the properties

                    _queryPartsAggregator.PropertyExtractionPart = N1QlHelpers.EscapeIdentifier("result");
                    ScalarResultBehavior.ResultExtractionRequired = true;
                }
            }

            return expression;
        }

        /// <summary>
        /// Provide a SELECT clause to returns all extents from the query
        /// </summary>
        /// <returns></returns>
        private string GetExtentSelectParameters()
        {
            IEnumerable<string> extents = _queryPartsAggregator.FromParts.Select(p => p.ItemName);

            if (_queryPartsAggregator.LetParts != null)
            {
                extents = extents.Concat(_queryPartsAggregator.LetParts.Select(p => p.ItemName));
            }
            return string.Join(", ", extents);
        }

        /// <summary>
        /// Provides the string to append to the SELECT list if we need to select the document metadata.
        /// Otherwise returns an empty string.
        /// </summary>
        /// <param name="queryModel">Query model being visited.  Used to extract the MainFromClause.</param>
        private string SelectDocumentMetadataIfRequired(QueryModel queryModel)
        {
            if (_queryGenerationContext.SelectDocumentMetadata && (_queryPartsAggregator.QueryType == N1QlQueryType.Select))
            {
                // The query generator must be requesting the document metadata
                // And this must be the main query, not a sub query

                return string.Format(", META({0}) as `__metadata`",
                    GetN1QlExpression(new QuerySourceReferenceExpression(queryModel.MainFromClause)));
            }
            else
            {
                return "";
            }
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            if (_visitStatus != VisitStatus.AfterGroupSubquery)
            {
                _queryPartsAggregator.AddWherePart(GetN1QlExpression(whereClause.Predicate));
            }
            else
            {
                _queryPartsAggregator.AddHavingPart(GetN1QlExpression(whereClause.Predicate));
            }

            base.VisitWhereClause(whereClause, queryModel, index);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            if ((resultOperator is TakeResultOperator))
            {
                var takeResultOperator = resultOperator as TakeResultOperator;

                _queryPartsAggregator.AddLimitPart(" LIMIT {0}",
                    Convert.ToInt32(GetN1QlExpression(takeResultOperator.Count)));
            }
            else if (resultOperator is SkipResultOperator)
            {
                var skipResultOperator = resultOperator as SkipResultOperator;

                _queryPartsAggregator.AddOffsetPart(" OFFSET {0}",
                    Convert.ToInt32(GetN1QlExpression(skipResultOperator.Count)));
            }
            else if (resultOperator is FirstResultOperator)
            {
                // We can save query execution time with a short circuit for .First()

                _queryPartsAggregator.AddLimitPart(" LIMIT {0}", 1);
            }
            else if (resultOperator is SingleResultOperator)
            {
                // We can save query execution time with a short circuit for .Single()
                // But we have to get at least 2 results so we know if there was more than 1

                _queryPartsAggregator.AddLimitPart(" LIMIT {0}", 2);
            }
            else if (resultOperator is DistinctResultOperator)
            {
                var distinctResultOperator = resultOperator as DistinctResultOperator;
                _queryPartsAggregator.AddDistinctPart("DISTINCT ");
            }
            else if (resultOperator is ExplainResultOperator)
            {
                _queryPartsAggregator.ExplainPart = "EXPLAIN ";
            }
            else if (resultOperator is ToQueryRequestResultOperator)
            {
                // Do nothing, conversion will be handled by BucketQueryExecutor
            }
            else if (resultOperator is AnyResultOperator)
            {
                _queryPartsAggregator.QueryType =
                    _queryPartsAggregator.QueryType == N1QlQueryType.Array ? N1QlQueryType.ArrayAny :
                        _queryPartsAggregator.QueryType == N1QlQueryType.Subquery ? N1QlQueryType.SubqueryAny : N1QlQueryType.MainQueryAny;

                if (_queryPartsAggregator.QueryType == N1QlQueryType.SubqueryAny)
                {
                    // For any Any query this value won't be used
                    // But we'll generate it for consistency

                    _queryPartsAggregator.PropertyExtractionPart =
                        _queryGenerationContext.ExtentNameProvider.GetUnlinkedExtentName();
                }
                else if (_queryPartsAggregator.QueryType == N1QlQueryType.MainQueryAny)
                {
                    // Result must be extracted from the result attribute on the returned JSON document
                    // If no rows are returned, for an Any operation we should return false.

                    ScalarResultBehavior.ResultExtractionRequired = true;
                    ScalarResultBehavior.NoRowsResult = false;
                }
            }
            else if (resultOperator is AllResultOperator)
            {
                _queryPartsAggregator.QueryType =
                    _queryPartsAggregator.QueryType == N1QlQueryType.Array ? N1QlQueryType.ArrayAll :
                        _queryPartsAggregator.QueryType == N1QlQueryType.Subquery ? N1QlQueryType.SubqueryAll : N1QlQueryType.MainQueryAll;

                bool prefixedExtents = false;
                if (_queryPartsAggregator.QueryType == N1QlQueryType.SubqueryAll)
                {
                    // We're putting allResultOperator.Predicate in the SATISFIES clause of an ALL clause
                    // Each extent of the subquery will be a property returned by the subquery
                    // So we need to prefix the references to the subquery in the predicate with the iterator name from the ALL clause

                    _queryPartsAggregator.PropertyExtractionPart =
                        _queryGenerationContext.ExtentNameProvider.GetUnlinkedExtentName();

                    prefixedExtents = true;
                    _queryGenerationContext.ExtentNameProvider.Prefix = _queryPartsAggregator.PropertyExtractionPart + ".";
                }
                else if (_queryPartsAggregator.QueryType == N1QlQueryType.ArrayAll)
                {
                    // We're dealing with an array-type subquery
                    // If there is any pre-filtering on the array using a Where clause, these statements will be
                    // referencing the query source using the default extent name.  When we apply the SATISFIES clause
                    // we'll need to use a new extent name to reference the results of the internal WHERE clause.

                    _queryPartsAggregator.PropertyExtractionPart =
                        _queryGenerationContext.ExtentNameProvider.GenerateNewExtentName(queryModel.MainFromClause);
                }
                else if (_queryPartsAggregator.QueryType == N1QlQueryType.MainQueryAll)
                {
                    // Result must be extracted from the result attribute on the returned JSON document
                    // If no rows are returned, for an All operation we should return true.

                    ScalarResultBehavior.ResultExtractionRequired = true;
                    ScalarResultBehavior.NoRowsResult = true;
                }

                var allResultOperator = (AllResultOperator) resultOperator;
                _queryPartsAggregator.WhereAllPart = GetN1QlExpression(allResultOperator.Predicate);

                if (prefixedExtents)
                {
                    _queryGenerationContext.ExtentNameProvider.Prefix = null;
                }
            }
            else if (resultOperator is ContainsResultOperator)
            {
                if (_queryPartsAggregator.QueryType != N1QlQueryType.Array)
                {
                    throw new NotSupportedException("Contains is only supported in N1QL against nested or constant arrays.");
                }

                var containsResultOperator = (ContainsResultOperator) resultOperator;

                // Use a wrapping function to wrap the subquery with an IN statement

                _queryPartsAggregator.AddWrappingFunction(GetN1QlExpression(containsResultOperator.Item) + " IN ");
            }
            else if (resultOperator is GroupResultOperator)
            {
                VisitGroupResultOperator((GroupResultOperator)resultOperator, queryModel);
            }
            else if (resultOperator is AverageResultOperator)
            {
                _queryPartsAggregator.AggregateFunction = "AVG";
                _isAggregated = true;
            }
            else if ((resultOperator is CountResultOperator) || (resultOperator is LongCountResultOperator))
            {
                _queryPartsAggregator.AggregateFunction = "COUNT";
                _isAggregated = true;
            }
            else if (resultOperator is MaxResultOperator)
            {
                _queryPartsAggregator.AggregateFunction = "MAX";
                _isAggregated = true;
            }
            else if (resultOperator is MinResultOperator)
            {
                _queryPartsAggregator.AggregateFunction = "MIN";
                _isAggregated = true;
            }
            else if (resultOperator is SumResultOperator)
            {
                _queryPartsAggregator.AggregateFunction = "SUM";
                _isAggregated = true;
            }
            else if (resultOperator is UnionResultOperator)
            {
                EnsureNotArraySubquery();

                var source = ((UnionResultOperator) resultOperator).Source2 as SubQueryExpression;
                if (source == null)
                {
                    throw new NotSupportedException("Union is only support against query sources.");
                }

                VisitUnion(source, true);
            }
            else if (resultOperator is ConcatResultOperator)
            {
                EnsureNotArraySubquery();

                var source = ((ConcatResultOperator)resultOperator).Source2 as SubQueryExpression;
                if (source == null)
                {
                    throw new NotSupportedException("Concat is only support against query sources.");
                }

                VisitUnion(source, false);
            }
            else
            {
                throw new NotSupportedException(string.Format("{0} is not supported.", resultOperator.GetType().Name));
            }

            base.VisitResultOperator(resultOperator, queryModel, index);
        }

        private void VisitUnion(SubQueryExpression source, bool distinct)
        {
            var queryModelVisitor = new N1QlQueryModelVisitor(_queryGenerationContext.CloneForUnion());

            queryModelVisitor.VisitQueryModel(source.QueryModel);
            var unionQuery = queryModelVisitor.GetQuery();

            _queryPartsAggregator.AddUnionPart((distinct ? " UNION " : " UNION ALL ") + unionQuery);
        }

        #region Grouping

        protected virtual void VisitGroupResultOperator(GroupResultOperator groupResultOperator, QueryModel queryModel)
        {
            _groupingExpressionTransformerRegistry = new ExpressionTransformerRegistry();

            // Add GROUP BY clause for the grouping key
            // And add transformations for any references to the key

            if (groupResultOperator.KeySelector.NodeType == ExpressionType.New)
            {
                // Grouping by a multipart key, so add each key to the GROUP BY clause

                var newExpression = (NewExpression) groupResultOperator.KeySelector;

                foreach (var argument in newExpression.Arguments)                {
                    _queryPartsAggregator.AddGroupByPart(GetN1QlExpression(argument));
                }

                // Use MultiKeyExpressionTransformer to remap access to the Key property

                _groupingExpressionTransformerRegistry.Register(
                    new MultiKeyExpressionTransfomer(_queryGenerationContext.GroupingQuerySource, newExpression));
            }
            else
            {
                // Grouping by a single column

                _queryPartsAggregator.AddGroupByPart(GetN1QlExpression(groupResultOperator.KeySelector));

                // Use KeyExpressionTransformer to remap access to the Key property

                _groupingExpressionTransformerRegistry.Register(
                    new KeyExpressionTransfomer(_queryGenerationContext.GroupingQuerySource, groupResultOperator.KeySelector));
            }

            // Add transformations for any references to the element selector

            var querySource = groupResultOperator.ElementSelector as QuerySourceReferenceExpression;
            if (querySource != null)
            {
                _queryGenerationContext.ExtentNameProvider.LinkExtents(
                    querySource.ReferencedQuerySource,
                    _queryGenerationContext.GroupingQuerySource.ReferencedQuerySource);
            }
            else
            {
                throw new NotSupportedException("Unsupported GroupResultOperator ElementSelector Type");
            }
        }

        #endregion

        #region Order By Clauses

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            if (_visitStatus == VisitStatus.InGroupSubquery)
            {
                // Just ignore sorting before grouping takes place
                return;
            }

            if (_queryPartsAggregator.QueryType != N1QlQueryType.Array)
            {
                var orderByParts =
                    orderByClause.Orderings.Select(
                        ordering =>
                            string.Concat(GetN1QlExpression(ordering.Expression), " ",
                                ordering.OrderingDirection.ToString().ToUpper())).ToList();

                _queryPartsAggregator.AddOrderByPart(orderByParts);

                base.VisitOrderByClause(orderByClause, queryModel, index);
            }
            else
            {
                // This is an array subquery

                if (!VerifyArraySubqueryOrderByClause(orderByClause, queryModel, index))
                {
                    throw new NotSupportedException("N1Ql Array Subqueries Support One Ordering By The Array Elements Only");
                }

                _queryPartsAggregator.AddWrappingFunction("ARRAY_SORT");
                if (orderByClause.Orderings[0].OrderingDirection == OrderingDirection.Desc)
                {
                    // There is no function to sort an array descending
                    // so we just reverse the array after it's sorted ascending

                    _queryPartsAggregator.AddWrappingFunction("ARRAY_REVERSE");
                }
            }
        }

        private bool VerifyArraySubqueryOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            // Arrays can only be sorted by a single ordering

            if ((index > 0) || (orderByClause.Orderings.Count() != 1))
            {
                return false;
            }

            // Array must be ordered by the main from expression
            // Which means the array elements themselves

            var querySourceReferenceExpression = orderByClause.Orderings[0].Expression as QuerySourceReferenceExpression;
            if (querySourceReferenceExpression == null)
            {
                return false;
            }

            var referencedQuerySource = querySourceReferenceExpression.ReferencedQuerySource as FromClauseBase;
            if (referencedQuerySource == null)
            {
                return false;
            }

            if (referencedQuerySource.FromExpression != queryModel.MainFromClause.FromExpression)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Additional From Clauses

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            EnsureNotArraySubquery();

            var handled = false;

            if (fromClause.FromExpression.NodeType == ExpressionType.MemberAccess)
            {
                // Unnest operation

                var fromPart = VisitMemberFromExpression(fromClause, fromClause.FromExpression as MemberExpression);
                _queryPartsAggregator.AddFromPart(fromPart);
                handled = true;
            }
            else if (fromClause.FromExpression is SubQueryExpression)
            {
                // Might be an unnest or a join to another bucket

                handled = VisitSubQueryFromExpression(fromClause, (SubQueryExpression) fromClause.FromExpression);
            }

            if (!handled)
            {
                throw new NotSupportedException("N1QL Does Not Support This Type Of From Clause");
            }

            base.VisitAdditionalFromClause(fromClause, queryModel, index);
        }

        /// <summary>
        /// Visits an AdditionalFromClause that is executing a subquery
        /// </summary>
        /// <param name="fromClause">AdditionalFromClause being visited</param>
        /// <param name="subQuery">Subquery being executed by the AdditionalFromClause</param>
        /// <returns>True if handled</returns>
        private bool VisitSubQueryFromExpression(AdditionalFromClause fromClause, SubQueryExpression subQuery)
        {
            var mainFromExpression = subQuery.QueryModel.MainFromClause.FromExpression;

            if (mainFromExpression is QuerySourceReferenceExpression)
            {
                // Joining to another bucket using a previous group join operation

                return VisitSubQuerySourceReferenceExpression(fromClause, subQuery,
                    (QuerySourceReferenceExpression) mainFromExpression);
            }
            else if (mainFromExpression.NodeType == ExpressionType.MemberAccess)
            {
                // Unnest operation

                var fromPart = VisitMemberFromExpression(fromClause, mainFromExpression as MemberExpression);

                if (subQuery.QueryModel.ResultOperators.OfType<DefaultIfEmptyResultOperator>().Any())
                {
                    fromPart.JoinType = "OUTER UNNEST";
                }

                _queryPartsAggregator.AddFromPart(fromPart);

                // be sure the subquery clauses use the same extent name
                _queryGenerationContext.ExtentNameProvider.LinkExtents(fromClause, subQuery.QueryModel.MainFromClause);

                // Apply where filters in the subquery to the main query
                VisitBodyClauses(subQuery.QueryModel.BodyClauses, subQuery.QueryModel);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Visit an AdditionalFromClause referencing a previous group join clause
        /// </summary>
        /// <param name="fromClause">AdditionalFromClause being visited</param>
        /// <param name="subQuery">SubQueryExpression being visited</param>
        /// <param name="querySourceReference">QuerySourceReferenceExpression that is the MainFromClause of the SubQuery</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator.  JoinType is defaulted to INNER UNNEST.</returns>
        private bool VisitSubQuerySourceReferenceExpression(AdditionalFromClause fromClause, SubQueryExpression subQuery,
            QuerySourceReferenceExpression querySourceReference)
        {
            var unclaimedJoin =
                    _unclaimedGroupJoins.FirstOrDefault(
                        p => p.GroupJoinClause == querySourceReference.ReferencedQuerySource);
            if (unclaimedJoin != null)
            {
                // this additional from clause is for a previous group join
                // if not, then it isn't supported and we'll let the method return false so an exception is thrown

                var fromPart = ParseJoinClause(unclaimedJoin.JoinClause);

                if (subQuery.QueryModel.ResultOperators.OfType<DefaultIfEmptyResultOperator>().Any())
                {
                    fromPart.JoinType = "LEFT JOIN";

                    // TODO Handle where clauses applied to the inner sequence before the join
                    // Currently they are filtered after the join is complete instead of before by N1QL
                }

                // Be sure that any reference to the subquery gets the join clause extent name
                _queryGenerationContext.ExtentNameProvider.LinkExtents(unclaimedJoin.JoinClause, fromClause);

                _unclaimedGroupJoins.Remove(unclaimedJoin);
                _queryPartsAggregator.AddFromPart(fromPart);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Visit an AdditionalFromClause referencing a member
        /// </summary>
        /// <param name="fromClause">AdditionalFromClause being visited</param>
        /// <param name="expression">MemberExpression being referenced</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator.  JoinType is defaulted to INNER UNNEST.</returns>
        private N1QlFromQueryPart VisitMemberFromExpression(AdditionalFromClause fromClause, MemberExpression expression)
        {
            // This case represents an unnest operation

            return new N1QlFromQueryPart()
            {
                Source = GetN1QlExpression(expression),
                ItemName = GetExtentName(fromClause),
                JoinType = "INNER UNNEST"
            };
        }

        #endregion

        #region Join Clauses

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel,
            GroupJoinClause groupJoinClause)
        {
            // Store the group join with the expectation it will be used later by an additional from clause

            EnsureNotArraySubquery();

            _unclaimedGroupJoins.Add(new UnclaimedGroupJoin()
            {
                JoinClause = joinClause,
                GroupJoinClause = groupJoinClause
            });

            base.VisitJoinClause(joinClause, queryModel, groupJoinClause);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            // basic join clause is an INNER JOIN against another bucket

            EnsureNotArraySubquery();

            var fromQueryPart = ParseJoinClause(joinClause);

            _queryPartsAggregator.AddFromPart(fromQueryPart);

            base.VisitJoinClause(joinClause, queryModel, index);
        }

        /// <summary>
        /// Visits a join against either a constant expression of IBucketQueryable, or a subquery based on an IBucketQueryable
        /// </summary>
        /// <param name="joinClause">Join clause being visited</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator.  JoinType is defaulted to INNER JOIN.</returns>
        /// <remarks>The InnerKeySelector must be selecting the N1QlFunctions.Key of the InnerSequence</remarks>
        private N1QlFromQueryPart ParseJoinClause(JoinClause joinClause)
        {
            if (joinClause.InnerSequence.NodeType == ExpressionType.Constant)
            {
                return VisitConstantExpressionJoinClause(joinClause, joinClause.InnerSequence as ConstantExpression);
            }
            else if (joinClause.InnerSequence is SubQueryExpression)
            {
                var subQuery = (SubQueryExpression) joinClause.InnerSequence;
                if (subQuery.QueryModel.ResultOperators.Any() ||
                    subQuery.QueryModel.MainFromClause.FromExpression.NodeType != ExpressionType.Constant)
                {
                    throw new NotSupportedException("Unsupported Join Inner Sequence");
                }

                // be sure the subquery clauses use the same name
                _queryGenerationContext.ExtentNameProvider.LinkExtents(joinClause,
                    subQuery.QueryModel.MainFromClause);

                var fromPart = VisitConstantExpressionJoinClause(joinClause,
                    subQuery.QueryModel.MainFromClause.FromExpression as ConstantExpression);

                VisitBodyClauses(subQuery.QueryModel.BodyClauses, subQuery.QueryModel);

                return fromPart;
            }
            else
            {
                throw new NotSupportedException("Unsupported Join Inner Sequence");
            }
        }

        /// <summary>
        /// Visits a join against a constant expression, which must be an IBucketQueryable implementation
        /// </summary>
        /// <param name="joinClause">Join clause being visited</param>
        /// <param name="constantExpression">Constant expression that is the InnerSequence of the JoinClause</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator.  JoinType is defaulted to INNER JOIN.</returns>
        /// <remarks>The InnerKeySelector must be selecting the N1QlFunctions.Key of the InnerSequence</remarks>
        private N1QlFromQueryPart VisitConstantExpressionJoinClause(JoinClause joinClause, ConstantExpression constantExpression)
        {
            string bucketName = null;

            if (constantExpression != null)
            {
                var bucketQueryable = constantExpression.Value as IBucketQueryable;
                if (bucketQueryable != null)
                {
                    bucketName = bucketQueryable.BucketName;
                }
            }

            if (bucketName == null)
            {
                throw new NotSupportedException("N1QL Joins Must Be Against IBucketQueryable");
            }

            IQuerySource keyQuerySource;
            if (!IsKeyMethodCall(joinClause.InnerKeySelector, out keyQuerySource))
            {
                if (_queryGenerationContext.ClusterVersion < Versioning.FeatureVersions.IndexJoin)
                {
                    throw new NotSupportedException(
                        "N1QL Join Selector Must Be A Call To N1QlFunctions.Key Referencing The Inner Sequence");
                }
                else
                {
                    if (!IsKeyMethodCall(joinClause.OuterKeySelector, out keyQuerySource))
                    {
                        throw new NotSupportedException(
                            "N1QL Join Selector Must Be A Call To N1QlFunctions.Key Referencing One Of The Sequences");
                    }
                    else
                    {
                        // Index join
                        return new N1QlFromQueryPart()
                        {
                            Source = N1QlHelpers.EscapeIdentifier(bucketName),
                            ItemName = GetExtentName(joinClause),
                            OnKeys = GetN1QlExpression(joinClause.InnerKeySelector),
                            JoinType = "INNER JOIN",
                            IndexJoinExtentName = GetExtentName(keyQuerySource)
                        };
                    }
                }
            }

            // Regular join
            return new N1QlFromQueryPart()
            {
                Source = N1QlHelpers.EscapeIdentifier(bucketName),
                ItemName = GetExtentName(joinClause),
                OnKeys = GetN1QlExpression(joinClause.OuterKeySelector),
                JoinType = "INNER JOIN"
            };
        }

        #endregion

        #region Nest Clause

        public void VisitNestClause(NestClause nestClause, QueryModel queryModel, int index)
        {
            EnsureNotArraySubquery();

            _queryPartsAggregator.AddFromPart(ParseNestClause(nestClause));
        }

        /// <summary>
        /// Visits a nest against either a constant expression of IBucketQueryable, or a subquery based on an IBucketQueryable
        /// </summary>
        /// <param name="nestClause">Nest clause being visited</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator</returns>
        private N1QlFromQueryPart ParseNestClause(NestClause nestClause)
        {
            if (nestClause.InnerSequence.NodeType == ExpressionType.Constant)
            {
                return VisitConstantExpressionNestClause(nestClause, nestClause.InnerSequence as ConstantExpression,
                    GetExtentName(nestClause));
            }
            else if (nestClause.InnerSequence is SubQueryExpression)
            {
                var subQuery = (SubQueryExpression) nestClause.InnerSequence;
                if (subQuery.QueryModel.ResultOperators.Any() ||
                    subQuery.QueryModel.MainFromClause.FromExpression.NodeType != ExpressionType.Constant)
                {
                    throw new NotSupportedException("Unsupported Nest Inner Sequence");
                }

                // Generate a temporary item name to use on the NEST statement, which we can then reference in the LET statement

                var genItemName = _queryGenerationContext.ExtentNameProvider.GetUnlinkedExtentName();
                var fromPart = VisitConstantExpressionNestClause(nestClause,
                    subQuery.QueryModel.MainFromClause.FromExpression as ConstantExpression, genItemName);

                // Put any where clauses in the sub query in an ARRAY filtering clause using a LET statement

                var whereClauseString = string.Join(" AND ",
                    subQuery.QueryModel.BodyClauses.OfType<WhereClause>()
                        .Select(p => GetN1QlExpression(p.Predicate)));

                var letPart = new N1QlLetQueryPart()
                {
                    ItemName = GetExtentName(nestClause),
                    Value =
                        string.Format("ARRAY {0} FOR {0} IN {1} WHEN {2} END",
                            GetExtentName(subQuery.QueryModel.MainFromClause),
                            genItemName,
                            whereClauseString)
                };

                _queryPartsAggregator.AddLetPart(letPart);

                if (!nestClause.IsLeftOuterNest)
                {
                    // This is an INNER NEST, but the inner sequence filter is being applied after the NEST operation is done
                    // So we need to put an additional filter to drop rows with an empty array result

                    _queryPartsAggregator.AddWherePart("(ARRAY_LENGTH({0}) > 0)", letPart.ItemName);
                }

                return fromPart;
            }
            else
            {
                throw new NotSupportedException("Unsupported Nest Inner Sequence");
            }
        }

        /// <summary>
        /// Visits a nest against a constant expression, which must be an IBucketQueryable implementation
        /// </summary>
        /// <param name="nestClause">Nest clause being visited</param>
        /// <param name="constantExpression">Constant expression that is the InnerSequence of the NestClause</param>
        /// <param name="itemName">Name to be used when referencing the data being nested</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator</returns>
        private N1QlFromQueryPart VisitConstantExpressionNestClause(NestClause nestClause, ConstantExpression constantExpression, string itemName)
        {
            string bucketName = null;

            if (constantExpression != null)
            {
                var bucketQueryable = constantExpression.Value as IBucketQueryable;
                if (bucketQueryable != null)
                {
                    bucketName = bucketQueryable.BucketName;
                }
            }

            if (bucketName == null)
            {
                throw new NotSupportedException("N1QL Nests Must Be Against IBucketQueryable");
            }

            return new N1QlFromQueryPart()
            {
                Source = N1QlHelpers.EscapeIdentifier(bucketName),
                ItemName = itemName,
                OnKeys = GetN1QlExpression(nestClause.KeySelector),
                JoinType = nestClause.IsLeftOuterNest ? "LEFT OUTER NEST" : "INNER NEST"
            };
        }

        /// <summary>
        /// Visits an index nest join against either a constant expression of IBucketQueryable, or a subquery based on an IBucketQueryable
        /// </summary>
        /// <param name="joinClause">Join clause being visited</param>
        /// <param name="groupJoinClause">Group join clause being visited</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator.  JoinType is defaulted to NEST.</returns>
        /// <remarks>The OuterKeySelector must be selecting the N1QlFunctions.Key of the OuterSequence</remarks>
        private N1QlFromQueryPart ParseIndexNestJoinClause(JoinClause joinClause, GroupJoinClause groupJoinClause)
        {
            if (joinClause.InnerSequence.NodeType == ExpressionType.Constant)
            {
                var clause = VisitConstantExpressionJoinClause(joinClause, joinClause.InnerSequence as ConstantExpression);
                clause.JoinType = "LEFT OUTER NEST";

                _queryGenerationContext.ExtentNameProvider.LinkExtents(joinClause, groupJoinClause);

                return clause;
            }
            else if (joinClause.InnerSequence is SubQueryExpression)
            {
                var subQuery = (SubQueryExpression)joinClause.InnerSequence;
                if (subQuery.QueryModel.ResultOperators.Any() ||
                    subQuery.QueryModel.MainFromClause.FromExpression.NodeType != ExpressionType.Constant)
                {
                    throw new NotSupportedException("Unsupported Join Inner Sequence");
                }

                // Generate a temporary item name to use on the NEST statement, which we can then reference in the LET statement

                var fromPart = VisitConstantExpressionJoinClause(joinClause,
                    subQuery.QueryModel.MainFromClause.FromExpression as ConstantExpression);
                fromPart.JoinType = "LEFT OUTER NEST";

                // Put any where clauses in the sub query in an ARRAY filtering clause using a LET statement

                var whereClauseString = string.Join(" AND ",
                    subQuery.QueryModel.BodyClauses.OfType<WhereClause>()
                        .Select(p => GetN1QlExpression(p.Predicate)));

                var letPart = new N1QlLetQueryPart()
                {
                    ItemName = GetExtentName(groupJoinClause),
                    Value =
                        string.Format("ARRAY {0} FOR {0} IN {1} WHEN {2} END",
                            GetExtentName(subQuery.QueryModel.MainFromClause),
                            GetExtentName(joinClause),
                            whereClauseString)
                };

                _queryPartsAggregator.AddLetPart(letPart);

                return fromPart;
            }
            else
            {
                throw new NotSupportedException("Unsupported Join Inner Sequence");
            }
        }

        #endregion

        /// <summary>
        /// Determines if the expression is a valid call to <see cref="N1QlFunctions.Key"/>, with an
        /// <see cref="IQuerySource"/> as the parameter.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="querySource">Returns the <see cref="IQuerySource"/> parameter for the call.</param>
        /// <returns>True if the call is valid.</returns>
        private static bool IsKeyMethodCall(Expression expression, out IQuerySource querySource)
        {
            querySource = null;

            var keyExpression = expression as MethodCallExpression;
            if ((keyExpression == null) ||
                (keyExpression.Method != typeof(N1QlFunctions).GetMethod("Key")) ||
                (keyExpression.Arguments.Count != 1))
            {
                return false;
            }

            var querySourceReference = keyExpression.Arguments[0] as QuerySourceReferenceExpression;
            if (querySourceReference == null)
            {
                return false;
            }

            querySource = querySourceReference.ReferencedQuerySource;
            return true;
        }

        private string GetN1QlExpression(Expression expression)
        {
            if (_visitStatus == VisitStatus.AfterGroupSubquery)
            {
                // SELECT, HAVING, and ORDER BY clauses must be remapped to refer directly to the extents in the grouping subquery
                // rather than refering to the output of the grouping subquery

                expression = TransformingExpressionVisitor.Transform(expression, _groupingExpressionTransformerRegistry);
            }

            return N1QlExpressionTreeVisitor.GetN1QlExpression(expression, _queryGenerationContext);
        }

        private string GetExtentName(IQuerySource querySource)
        {
            return _queryGenerationContext.ExtentNameProvider.GetExtentName(querySource);
        }

        private void EnsureNotArraySubquery()
        {
            if (_queryPartsAggregator.IsArraySubquery)
            {
                throw new NotSupportedException("N1QL Array Subqueries Do Not Support Joins, Nests, Union, Concat, Or Additional From Statements");
            }
        }
    }
}