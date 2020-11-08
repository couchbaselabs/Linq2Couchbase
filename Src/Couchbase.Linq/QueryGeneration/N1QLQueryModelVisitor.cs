using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Core.IO.Serializers;
using Couchbase.Linq.Clauses;
using Couchbase.Linq.Execution;
using Couchbase.Linq.Operators;
using Couchbase.Linq.QueryGeneration.ExpressionTransformers;
using Couchbase.Linq.QueryGeneration.FromParts;
using Couchbase.Linq.QueryGeneration.MemberNameResolvers;
using Couchbase.Linq.Versioning;
using Microsoft.Extensions.Logging;
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
        private readonly QueryPartsAggregator _queryPartsAggregator;

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
        private ExpressionTransformerRegistry? _groupingExpressionTransformerRegistry;

        /// <summary>
        /// Provides information about how scalar results should be extracted from the N1QL query result after execution.
        /// </summary>
        public ScalarResultBehavior ScalarResultBehavior { get; } = new ScalarResultBehavior();

        public N1QlQueryModelVisitor(N1QlQueryGenerationContext queryGenerationContext) : this(queryGenerationContext, false)
        {
        }

        /// <exception cref="ArgumentNullException"><paramref name="queryGenerationContext"/> is <see langword="null" />.</exception>
        public N1QlQueryModelVisitor(N1QlQueryGenerationContext queryGenerationContext, bool isSubQuery)
        {
            _queryGenerationContext = queryGenerationContext ?? throw new ArgumentNullException(nameof(queryGenerationContext));
            _isSubQuery = isSubQuery;

            _queryPartsAggregator = new QueryPartsAggregator(queryGenerationContext.LoggerFactory.CreateLogger<QueryPartsAggregator>());

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
                typeof(ICollectionQueryable).GetTypeInfo().IsAssignableFrom(bucketConstantExpression.Type))
            {
                if (_isSubQuery && !queryModel.BodyClauses.Any(p => p is UseKeysClause))
                {
                    throw new NotSupportedException("N1Ql Bucket Subqueries Require A UseKeys Call");
                }

                _queryPartsAggregator.AddExtent(new FromPart(fromClause)
                {
                    Source = N1QlHelpers.EscapeIdentifier(((ICollectionQueryable) bucketConstantExpression.Value).BucketName),
                    ItemName = GetExtentName(fromClause)
                });
            }
            else if (fromClause.FromExpression.NodeType == ExpressionType.MemberAccess)
            {
                if (!_isSubQuery)
                {
                    throw new NotSupportedException("Member Access In The Main From Clause Is Only Supported In Subqueries");
                }

                _queryPartsAggregator.AddExtent(new FromPart(fromClause)
                {
                    Source = GetN1QlExpression((MemberExpression) fromClause.FromExpression),
                    ItemName = GetExtentName(fromClause)
                });

                // This is an Array type subquery, since we're querying against a member not a bucket
                _queryPartsAggregator.QueryType = N1QlQueryType.Array;
            }
            else if (fromClause.FromExpression is SubQueryExpression subQueryExpression)
            {
                VisitSubQueryFromClause(fromClause, subQueryExpression);
            }
            else if (fromClause.FromExpression is QuerySourceReferenceExpression querySourceReferenceExpression)
            {
                if (querySourceReferenceExpression.ReferencedQuerySource is GroupJoinClause)
                {
                    // This is an array subquery against a NEST clause
                    VisitArrayFromClause(fromClause);
                }
                else if (fromClause.FromExpression.Equals(_queryGenerationContext.GroupingQuerySource))
                {
                    // We're performing an aggregate against a group
                    _queryPartsAggregator.QueryType = N1QlQueryType.Aggregate;

                    // Ensure that we use the same extent name as the grouping
                    _queryGenerationContext.ExtentNameProvider.LinkExtents(
                        _queryGenerationContext.GroupingQuerySource.ReferencedQuerySource, fromClause);
                }
                else
                {
                    throw new NotSupportedException("From Clause Is Referencing An Invalid Query Source");
                }
            }
            else if (fromClause.FromExpression is ConstantExpression || fromClause.FromExpression is MethodCallExpression)
            {
                // From clause for this subquery is a constant array or a function returning an array

                VisitArrayFromClause(fromClause);
            }

            base.VisitMainFromClause(fromClause, queryModel);
        }

        private void VisitArrayFromClause(MainFromClause fromClause)
        {
            _queryPartsAggregator.QueryType = N1QlQueryType.Array;

            _queryPartsAggregator.AddExtent(new FromPart(fromClause)
            {
                Source = GetN1QlExpression(fromClause.FromExpression),
                ItemName = GetExtentName(fromClause)
            });
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

        public virtual void VisitHintClause(HintClause clause, QueryModel queryModel, int index)
        {
            VisitHintClause(clause, _queryPartsAggregator.Extents[0]);
        }

        public virtual void VisitHintClause(HintClause clause, ExtentPart fromPart)
        {
            if (fromPart.Hints == null)
            {
                fromPart.Hints = new List<HintClause>();
            }
            else if (fromPart.Hints.Any(p => p.GetType() == clause.GetType()))
            {
                throw new NotSupportedException($"Only one {clause.GetType().Name} is allowed per extent.");
            }

            if (clause is UseHashClause && !(fromPart is JoinPart))
            {
                throw new NotSupportedException("UseHash is only supported on joined extents");
            }

            fromPart.Hints.Add(clause);
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

        /// <summary>
        /// Builds select clause when we're directly referencing elements of a query extent.
        /// Could represent an array of documents or an array subdocument.
        /// </summary>
        private string GetQuerySourceSelectParameters(SelectClause selectClause, QueryModel queryModel)
        {
            if (_isAggregated)
            {
                // for aggregates, just use "*" (i.e. AggregateFunction = "COUNT", expression = "*" results in COUNT(*)"

                _queryPartsAggregator.RawSelection = true;

                if (_queryPartsAggregator.IsArraySubquery)
                {
                    // Can't use * in an array subquery
                    return GetN1QlExpression(selectClause.Selector);
                }
                else
                {
                    return "*";
                }
            }

            if (_queryPartsAggregator.IsArraySubquery)
            {
                // For unaggregated array subqueries, just select the array element directly
                // i.e. ARRAY p FOR p IN someArray WHEN p.x = 1 END
                // The select clause being the first "p" in the ARRAY

                return GetN1QlExpression(selectClause.Selector);
            }

            if (_queryGenerationContext.SelectDocumentMetadata &&
                _queryPartsAggregator.QueryType == N1QlQueryType.Select)
            {
                // We will also be selecting document metadata, so we can't use SELECT RAW

                return string.Concat(GetN1QlExpression(selectClause.Selector), ".*",
                    SelectDocumentMetadataIfRequired(queryModel));
            }

            // We can use SELECT RAW to get the result
            _queryPartsAggregator.RawSelection = true;
            return GetN1QlExpression(selectClause.Selector);
        }

        private string GetSelectParameters(SelectClause selectClause, QueryModel queryModel)
        {
            string expression;

            if (selectClause.Selector is QuerySourceReferenceExpression)
            {
                expression = GetQuerySourceSelectParameters(selectClause, queryModel);
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
                        _queryPartsAggregator.RawSelection = true;

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

                _queryPartsAggregator.RawSelection = true;
            }

            return expression;
        }

        /// <summary>
        /// Provide a SELECT clause to returns all extents from the query
        /// </summary>
        /// <returns></returns>
        private string GetExtentSelectParameters()
        {
            IEnumerable<string> extents = _queryPartsAggregator.Extents.Select(p => p.ItemName);

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
                var predicate = whereClause.Predicate;

                if (_queryPartsAggregator.Extents.Count > 1)
                {
                    // There is more than one extent, so one may be an INNER NEST
                    var innerNestDetectingVistor = new InnerNestDetectingExpressionVisitor(_queryPartsAggregator);
                    predicate = innerNestDetectingVistor.Visit(predicate);
                }

                _queryPartsAggregator.AddWherePart(GetN1QlExpression(predicate));
            }
            else
            {
                _queryPartsAggregator.AddHavingPart(GetN1QlExpression(whereClause.Predicate));
            }

            base.VisitWhereClause(whereClause, queryModel, index);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            switch (resultOperator)
            {
                case TakeResultOperator takeResultOperator:
                    _queryPartsAggregator.AddLimitPart(" LIMIT {0}",
                        Convert.ToInt32(GetN1QlExpression(takeResultOperator.Count)));
                    break;

                case SkipResultOperator skipResultOperator:
                    _queryPartsAggregator.AddOffsetPart(" OFFSET {0}",
                        Convert.ToInt32(GetN1QlExpression(skipResultOperator.Count)));
                    break;

                case FirstResultOperator _:
                case FirstAsyncResultOperator _:
                    // We can save query execution time with a short circuit for .First()

                    _queryPartsAggregator.AddLimitPart(" LIMIT {0}", 1);
                    break;

                case SingleResultOperator _:
                case SingleAsyncResultOperator _:
                    // We can save query execution time with a short circuit for .Single()
                    // But we have to get at least 2 results so we know if there was more than 1

                    _queryPartsAggregator.AddLimitPart(" LIMIT {0}", 2);
                    break;

                case DistinctResultOperator _:
                    _queryPartsAggregator.AddDistinctPart("DISTINCT ");
                    break;

                case ExplainResultOperator _:
                case ExplainAsyncResultOperator _:
                    _queryPartsAggregator.ExplainPart = "EXPLAIN ";
                    break;

                case AnyResultOperator _:
                case AnyAsyncResultOperator _:
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

                    break;

                case AllResultOperator _:
                case AllAsyncResultOperator _:
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

                    var predicate = resultOperator switch
                    {
                        AllResultOperator allResultOperator => allResultOperator.Predicate,
                        AllAsyncResultOperator allAsyncResultOperator => allAsyncResultOperator.Predicate,
                        _ => null
                    };
                    _queryPartsAggregator.WhereAllPart = GetN1QlExpression(predicate!);

                    if (prefixedExtents)
                    {
                        _queryGenerationContext.ExtentNameProvider.Prefix = null;
                    }

                    break;
                }

                case ContainsResultOperator containsResultOperator:
                    if (_queryPartsAggregator.QueryType != N1QlQueryType.Array)
                    {
                        throw new NotSupportedException(
                            "Contains is only supported in N1QL against nested or constant arrays.");
                    }

                    // Use a wrapping function to wrap the subquery with an IN statement

                    _queryPartsAggregator.AddWrappingFunction(GetN1QlExpression(containsResultOperator.Item) + " IN ");
                    break;

                case GroupResultOperator groupResultOperator:
                    VisitGroupResultOperator(groupResultOperator, queryModel);
                    break;

                case AverageResultOperator _:
                case AverageAsyncResultOperator _:
                    _queryPartsAggregator.AggregateFunction = "AVG";
                    _isAggregated = true;
                    break;

                case CountResultOperator _:
                case LongCountResultOperator _:
                case CountAsyncResultOperator _:
                case LongCountAsyncResultOperator _:
                    if (_queryPartsAggregator.IsArraySubquery)
                    {
                        _queryPartsAggregator.AddWrappingFunction("ARRAY_LENGTH");
                    }
                    else
                    {
                        _queryPartsAggregator.AggregateFunction = "COUNT";
                    }

                    _isAggregated = true;
                    break;

                case MaxResultOperator _:
                case MaxAsyncResultOperator _:
                    _queryPartsAggregator.AggregateFunction = "MAX";
                    _isAggregated = true;
                    break;

                case MinResultOperator _:
                case MinAsyncResultOperator _:
                    _queryPartsAggregator.AggregateFunction = "MIN";
                    _isAggregated = true;
                    break;

                case SumResultOperator _:
                case SumAsyncResultOperator _:
                    _queryPartsAggregator.AggregateFunction = "SUM";
                    _isAggregated = true;
                    break;

                case UnionResultOperator unionResultOperator:
                {
                    EnsureNotArraySubquery();

                    var source = unionResultOperator.Source2 as SubQueryExpression;
                    if (source == null)
                    {
                        throw new NotSupportedException("Union is only support against query sources.");
                    }

                    VisitUnion(source, true);
                    break;
                }

                case ConcatResultOperator concatResultOperator:
                {
                    EnsureNotArraySubquery();

                    var source = concatResultOperator.Source2 as SubQueryExpression;
                    if (source == null)
                    {
                        throw new NotSupportedException("Concat is only support against query sources.");
                    }

                    VisitUnion(source, false);
                    break;
                }

                default:
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

            // Validate the status of array subqueries
            if (_queryPartsAggregator.QueryType == N1QlQueryType.Array)
            {
                if (!VerifyArraySubqueryOrderByClause(orderByClause, queryModel, index))
                {
                    // Switch to an array subquery using SELECT ... FROM
                    _queryPartsAggregator.QueryType = N1QlQueryType.Subquery;
                }
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

            if (fromClause.FromExpression is MemberExpression memberExpression)
            {
                // Unnest operation

                var fromPart = VisitMemberFromExpression(fromClause, memberExpression);
                _queryPartsAggregator.AddExtent(fromPart);
                handled = true;
            }
            else if (fromClause.FromExpression is SubQueryExpression subQueryExpression)
            {
                // Might be an unnest or a join to another bucket

                handled = VisitSubQueryFromExpression(fromClause, subQueryExpression);
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

            if (mainFromExpression is QuerySourceReferenceExpression querySourceReferenceExpression)
            {
                // Joining to another bucket using a previous group join operation

                return VisitSubQuerySourceReferenceExpression(fromClause, subQuery, querySourceReferenceExpression);
            }
            else if (mainFromExpression is MemberExpression memberExpression)
            {
                // Unnest operation

                var fromPart = VisitMemberFromExpression(fromClause, memberExpression);

                if (subQuery.QueryModel.ResultOperators.OfType<DefaultIfEmptyResultOperator>().Any())
                {
                    fromPart.JoinType = JoinTypes.LeftUnnest;
                }

                _queryPartsAggregator.AddExtent(fromPart);

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
        /// <returns>True if the additional from clause is valid.</returns>
        private bool VisitSubQuerySourceReferenceExpression(AdditionalFromClause fromClause, SubQueryExpression subQuery,
            QuerySourceReferenceExpression querySourceReference)
        {
            var ansiNest = _queryPartsAggregator.Extents.OfType<JoinPart>()
                .FirstOrDefault(p => p.QuerySource == querySourceReference.ReferencedQuerySource);
            if (ansiNest != null)
            {
                // Convert the ANSI NEST to a JOIN because the additional from clause
                // is flattening the query

                ansiNest.JoinType = subQuery.QueryModel.ResultOperators.OfType<DefaultIfEmptyResultOperator>().Any()
                    ? JoinTypes.LeftJoin
                    : JoinTypes.InnerJoin;

                // Be sure that any reference to the subquery gets the join clause extent name
                _queryGenerationContext.ExtentNameProvider.LinkExtents(ansiNest.QuerySource, fromClause);

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
        private JoinPart VisitMemberFromExpression(AdditionalFromClause fromClause, MemberExpression expression)
        {
            // This case represents an unnest operation

            return new JoinPart(fromClause)
            {
                Source = GetN1QlExpression(expression),
                ItemName = GetExtentName(fromClause),
                JoinType = JoinTypes.InnerUnnest
            };
        }

        #endregion

        #region Join Clauses

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel,
            GroupJoinClause groupJoinClause)
        {
            EnsureNotArraySubquery();

            var fromQueryPart = ParseNestJoinClause(joinClause, groupJoinClause);
            _queryPartsAggregator.AddExtent(fromQueryPart);

            base.VisitJoinClause(joinClause, queryModel, groupJoinClause);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            // basic join clause is an INNER JOIN against another bucket

            EnsureNotArraySubquery();

            var fromQueryPart = ParseJoinClause(joinClause);

            _queryPartsAggregator.AddExtent(fromQueryPart);

            base.VisitJoinClause(joinClause, queryModel, index);
        }

        /// <summary>
        /// Visits a join against either a constant expression of IBucketQueryable, or a subquery based on an IBucketQueryable
        /// </summary>
        /// <param name="joinClause">Join clause being visited</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator.  JoinType is defaulted to INNER JOIN.</returns>
        /// <remarks>The InnerKeySelector must be selecting the N1QlFunctions.Key of the InnerSequence</remarks>
        private JoinPart ParseJoinClause(JoinClause joinClause)
        {
            if (joinClause.InnerSequence is ConstantExpression constantExpression)
            {
                return VisitConstantExpressionJoinClause(joinClause, constantExpression);
            }
            else if (joinClause.InnerSequence is SubQueryExpression subQuery)
            {
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

                // If the right hand extent is filtered the predicates must
                // be part of the ON statement rather than part of the general predicates.

                fromPart.AdditionalInnerPredicates = string.Join(" AND ",
                    subQuery.QueryModel.BodyClauses
                        .OfType<WhereClause>()
                        .Select(p => GetN1QlExpression(p.Predicate)));

                foreach (var hintClause in subQuery.QueryModel.BodyClauses.OfType<HintClause>())
                {
                    VisitHintClause(hintClause, fromPart);
                }

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
        private AnsiJoinPart VisitConstantExpressionJoinClause(JoinClause joinClause, ConstantExpression? constantExpression)
        {
            string? bucketName = null;

            if (constantExpression != null)
            {
                if (constantExpression.Value is ICollectionQueryable bucketQueryable)
                {
                    bucketName = bucketQueryable.BucketName;
                }
            }

            if (bucketName == null)
            {
                throw new NotSupportedException("N1QL Joins Must Be Against IBucketQueryable");
            }

            return new AnsiJoinPart(joinClause)
            {
                Source = N1QlHelpers.EscapeIdentifier(bucketName),
                ItemName = GetExtentName(joinClause),
                OuterKey = GetN1QlExpression(joinClause.OuterKeySelector),
                InnerKey = GetN1QlExpression(joinClause.InnerKeySelector),
                JoinType = JoinTypes.InnerJoin
            };
        }

        #endregion

        #region Nest Clause

        public void VisitNestClause(NestClause nestClause, QueryModel queryModel, int index)
        {
            EnsureNotArraySubquery();

            _queryPartsAggregator.AddExtent(ParseNestClause(nestClause));
        }

        /// <summary>
        /// Visits a nest against either a constant expression of IBucketQueryable, or a subquery based on an IBucketQueryable
        /// </summary>
        /// <param name="nestClause">Nest clause being visited</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator</returns>
        private JoinPart ParseNestClause(NestClause nestClause)
        {
            if (nestClause.InnerSequence is ConstantExpression constantExpression)
            {
                return VisitConstantExpressionNestClause(nestClause, constantExpression, false);
            }
            else if (nestClause.InnerSequence is SubQueryExpression subQuery)
            {
                if (subQuery.QueryModel.ResultOperators.Any() ||
                    subQuery.QueryModel.MainFromClause.FromExpression.NodeType != ExpressionType.Constant)
                {
                    throw new NotSupportedException("Unsupported Nest Inner Sequence");
                }

                var fromPart = VisitConstantExpressionNestClause(nestClause,
                    subQuery.QueryModel.MainFromClause.FromExpression as ConstantExpression, true);

                // Ensure that the extents are linked before processing the where clause
                // So they have the same name
                _queryGenerationContext.ExtentNameProvider.LinkExtents(nestClause, subQuery.QueryModel.MainFromClause);

                fromPart.AdditionalInnerPredicates = string.Join(" AND ",
                    subQuery.QueryModel.BodyClauses.OfType<WhereClause>()
                        .Select(p => GetN1QlExpression(p.Predicate)));

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
        /// <param name="isSubQuery">Indicates if this nest clause is a subquery which may require the use of a LET clause after the NEST</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator</returns>
        private AnsiJoinPart VisitConstantExpressionNestClause(NestClause nestClause, ConstantExpression? constantExpression, bool isSubQuery)
        {
            string? bucketName = null;
            if (constantExpression?.Value is ICollectionQueryable bucketQueryable)
            {
                bucketName = bucketQueryable.BucketName;
            }

            if (bucketName == null)
            {
                throw new NotSupportedException("N1QL Nests Must Be Against IBucketQueryable");
            }

            var itemName = _queryGenerationContext.ExtentNameProvider.GetExtentName(nestClause);

            return new AnsiJoinPart(nestClause)
            {
                Source = N1QlHelpers.EscapeIdentifier(bucketName),
                ItemName = itemName,
                JoinType = nestClause.IsLeftOuterNest ? JoinTypes.LeftNest : JoinTypes.InnerNest,
                InnerKey = GetN1QlExpression(nestClause.KeySelector),
                OuterKey = $"META({itemName}).id",
                Operator = "IN"
            };
        }

        /// <summary>
        /// Visits an nest join against either a constant expression of IBucketQueryable, or a subquery based on an IBucketQueryable
        /// </summary>
        /// <param name="joinClause">Join clause being visited</param>
        /// <param name="groupJoinClause">Group join clause being visited</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator.  JoinType is defaulted to NEST.</returns>
        /// <remarks>The OuterKeySelector must be selecting the N1QlFunctions.Key of the OuterSequence</remarks>
        private JoinPart ParseNestJoinClause(JoinClause joinClause, GroupJoinClause groupJoinClause)
        {
            if (joinClause.InnerSequence.NodeType == ExpressionType.Constant)
            {
                var clause = VisitConstantExpressionJoinClause(joinClause, joinClause.InnerSequence as ConstantExpression);
                clause.JoinType = JoinTypes.LeftNest;
                clause.QuerySource = groupJoinClause;

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
                fromPart.JoinType = JoinTypes.LeftNest;
                fromPart.QuerySource = groupJoinClause;

                // Ensure references to the join pass through to the group join
                _queryGenerationContext.ExtentNameProvider.LinkExtents(joinClause, groupJoinClause);
                _queryGenerationContext.ExtentNameProvider.LinkExtents(joinClause, subQuery.QueryModel.MainFromClause);

                // Put any where clauses in the sub query on the join
                fromPart.AdditionalInnerPredicates = string.Join(" AND ",
                    subQuery.QueryModel.BodyClauses.OfType<WhereClause>()
                        .Select(p => GetN1QlExpression(p.Predicate)));

                foreach (var hintClause in subQuery.QueryModel.BodyClauses.OfType<HintClause>())
                {
                    VisitHintClause(hintClause, fromPart);
                }

                return fromPart;
            }
            else
            {
                throw new NotSupportedException("Unsupported Join Inner Sequence");
            }
        }

        #endregion

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