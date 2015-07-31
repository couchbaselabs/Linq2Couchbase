using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Linq.Clauses;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Operators;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Couchbase.Linq.QueryGeneration
{
    public class N1QlQueryModelVisitor : QueryModelVisitorBase, IN1QlQueryModelVisitor
    {
        private readonly N1QlQueryGenerationContext _queryGenerationContext;
        private readonly QueryPartsAggregator _queryPartsAggregator = new QueryPartsAggregator();
        private readonly List<UnclaimedGroupJoin> _unclaimedGroupJoins = new List<UnclaimedGroupJoin>(); 

        private bool _isSubQuery = false;

        public N1QlQueryModelVisitor()
        {
            _queryGenerationContext = new N1QlQueryGenerationContext()
            {
                MemberNameResolver = new JsonNetMemberNameResolver(ClusterHelper.Get().Configuration.SerializationSettings.ContractResolver),
                MethodCallTranslatorProvider = new DefaultMethodCallTranslatorProvider()
            };
        }

        public N1QlQueryModelVisitor(N1QlQueryGenerationContext queryGenerationContext) : this(queryGenerationContext, false)
        {            
        }

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

        public static string GenerateN1QlQuery(QueryModel queryModel)
        {
            var visitor = new N1QlQueryModelVisitor();
            visitor.VisitQueryModel(queryModel);
            return visitor.GetQuery();
        }

        public string GetQuery()
        {
            return _queryPartsAggregator.BuildN1QlQuery();
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            queryModel.MainFromClause.Accept(this, queryModel);
            VisitBodyClauses(queryModel.BodyClauses, queryModel);
            VisitResultOperators(queryModel.ResultOperators, queryModel);

            // Select clause must be visited after the from clause and body clauses
            // This ensures that any extents are linked before being referenced in the select statement
            // Select clause must be visited after result operations because Any and All operators
            // May change how we handle the select clause
            queryModel.SelectClause.Accept(this, queryModel);

            if (_unclaimedGroupJoins.Any())
            {
                throw new NotSupportedException("N1QL Requires All Group Joins Have A Matching From Clause Subquery");
            }
        }

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

            base.VisitMainFromClause(fromClause, queryModel);
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
                expression = GetN1QlExpression(selectClause.Selector);

                if (_queryPartsAggregator.QueryType != N1QlQueryType.Array)
                {
                    expression = string.Concat(expression, ".*");
                }
            }
            else if (selectClause.Selector.NodeType == ExpressionType.New)
            {
                if (_queryPartsAggregator.QueryType != N1QlQueryType.Array)
                {
                    expression =
                        N1QlExpressionTreeVisitor.GetN1QlSelectNewExpression(selectClause.Selector as NewExpression,
                            _queryGenerationContext);
                }
                else
                {
                    expression = GetN1QlExpression(selectClause.Selector);
                }
            }
            else
            {
                expression = GetN1QlExpression(selectClause.Selector);

                if ((_queryPartsAggregator.QueryType == N1QlQueryType.Subquery) || (_queryPartsAggregator.QueryType == N1QlQueryType.Array))
                {
                    // For LINQ, this subquery is expected to return a list of the specific property being selected
                    // But N1QL will always return a list of objects with a single property
                    // So we need to use an ARRAY statement to convert the list

                    _queryPartsAggregator.ArrayPropertyExtractionPart = N1QlHelpers.EscapeIdentifier("result");

                    expression += " as " + _queryPartsAggregator.ArrayPropertyExtractionPart;
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

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            _queryPartsAggregator.AddWherePart(GetN1QlExpression(whereClause.Predicate));
            base.VisitWhereClause(whereClause, queryModel, index);
        }

        public void VisitWhereMissingClause(WhereMissingClause whereClause, QueryModel queryModel, int index)
        {
            var expression = GetN1QlExpression(whereClause.Predicate);
            _queryPartsAggregator.AddWhereMissingPart(String.Concat(expression, " IS MISSING"));
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
            else if (resultOperator is DistinctResultOperator)
            {
                var distinctResultOperator = resultOperator as DistinctResultOperator;
                _queryPartsAggregator.AddDistinctPart("DISTINCT ");
            }
            else if (resultOperator is ExplainResultOperator)
            {
                _queryPartsAggregator.ExplainPart = "EXPLAIN ";
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

                    _queryPartsAggregator.ArrayPropertyExtractionPart =
                        _queryGenerationContext.ExtentNameProvider.GetUnlinkedExtentName();
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

                    _queryPartsAggregator.ArrayPropertyExtractionPart =
                        _queryGenerationContext.ExtentNameProvider.GetUnlinkedExtentName();

                    prefixedExtents = true;
                    _queryGenerationContext.ExtentNameProvider.Prefix = _queryPartsAggregator.ArrayPropertyExtractionPart + ".";
                }

                var allResultOperator = (AllResultOperator) resultOperator;
                _queryPartsAggregator.WhereAllPart = GetN1QlExpression(allResultOperator.Predicate);

                if (prefixedExtents)
                {
                    _queryGenerationContext.ExtentNameProvider.Prefix = null;
                }
            }


            base.VisitResultOperator(resultOperator, queryModel, index);
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            var orderByParts =
                orderByClause.Orderings.Select(
                    ordering =>
                        String.Concat(GetN1QlExpression(ordering.Expression), " ",
                            ordering.OrderingDirection.ToString().ToUpper())).ToList();

            _queryPartsAggregator.AddOrderByPart(orderByParts);

            base.VisitOrderByClause(orderByClause, queryModel, index);
        }

        #region Additional From Clauses

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            var handled = false;

            switch (fromClause.FromExpression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    // Unnest operation

                    var fromPart = VisitMemberFromExpression(fromClause, fromClause.FromExpression as MemberExpression);
                    _queryPartsAggregator.AddFromPart(fromPart);
                    handled = true;
                    break;

                case SubQueryExpression.ExpressionType:
                    // Might be an unnest or a join to another bucket

                    handled = VisitSubQueryFromExpression(fromClause, fromClause.FromExpression as SubQueryExpression);
                    break;
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

            switch (mainFromExpression.NodeType)
            {
                case QuerySourceReferenceExpression.ExpressionType:
                    // Joining to another bucket using a previous group join operation

                    return VisitSubQuerySourceReferenceExpression(fromClause, subQuery, mainFromExpression as QuerySourceReferenceExpression);

                case ExpressionType.MemberAccess:
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

            var fromQueryPart = ParseJoinClause(joinClause);

            _queryPartsAggregator.AddFromPart(fromQueryPart);

            base.VisitJoinClause(joinClause, queryModel, index);
        }

        /// <summary>
        /// Visits a join against either a constant expression of IBucketQueryable, or a subquery based on an IBucketQueryable
        /// </summary>
        /// <param name="joinClause">Join clause being visited</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator.  JoinType is defaulted to INNER JOIN.</returns>
        /// <remarks>The InnerKeySelector must be selecting the N1Ql.Key of the InnerSequence</remarks>
        private N1QlFromQueryPart ParseJoinClause(JoinClause joinClause)
        {
            switch (joinClause.InnerSequence.NodeType)
            {
                case ExpressionType.Constant:
                    return VisitConstantExpressionJoinClause(joinClause, joinClause.InnerSequence as ConstantExpression);

                case SubQueryExpression.ExpressionType:
                    var subQuery = joinClause.InnerSequence as SubQueryExpression;
                    if ((subQuery == null) || subQuery.QueryModel.ResultOperators.Any() || subQuery.QueryModel.MainFromClause.FromExpression.NodeType != ExpressionType.Constant)
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

                default:
                    throw new NotSupportedException("Unsupported Join Inner Sequence");
            }
        }

        /// <summary>
        /// Visits a join against a constant expression, which must be an IBucketQueryable implementation
        /// </summary>
        /// <param name="joinClause">Join clause being visited</param>
        /// <param name="constantExpression">Constant expression that is the InnerSequence of the JoinClause</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator.  JoinType is defaulted to INNER JOIN.</returns>
        /// <remarks>The InnerKeySelector must be selecting the N1Ql.Key of the InnerSequence</remarks>
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

            var keyExpression = joinClause.InnerKeySelector as MethodCallExpression;
            if ((keyExpression == null) ||
                (keyExpression.Method != typeof(N1Ql).GetMethod("Key")) ||
                (keyExpression.Arguments.Count != 1))
            {
                throw new NotSupportedException("N1QL Join Selector Must Be A Call To N1Ql.Key");
            }

            if (!(keyExpression.Arguments[0] is QuerySourceReferenceExpression))
            {
                throw new NotSupportedException("N1QL Join Selector Call To N1Ql.Key Must Reference The Inner Sequence");
            }

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
            _queryPartsAggregator.AddFromPart(ParseNestClause(nestClause));
        }

        /// <summary>
        /// Visits a nest against either a constant expression of IBucketQueryable, or a subquery based on an IBucketQueryable
        /// </summary>
        /// <param name="nestClause">Nest clause being visited</param>
        /// <returns>N1QlFromQueryPart to be added to the QueryPartsAggregator</returns>
        private N1QlFromQueryPart ParseNestClause(NestClause nestClause)
        {
            switch (nestClause.InnerSequence.NodeType)
            {
                case ExpressionType.Constant:
                    return VisitConstantExpressionNestClause(nestClause, nestClause.InnerSequence as ConstantExpression,
                        GetExtentName(nestClause));

                case SubQueryExpression.ExpressionType: // SubQueryExpression
                    var subQuery = nestClause.InnerSequence as SubQueryExpression;
                    if ((subQuery == null) || subQuery.QueryModel.ResultOperators.Any() || subQuery.QueryModel.MainFromClause.FromExpression.NodeType != ExpressionType.Constant)
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

                default:
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

        #endregion

        private string GetN1QlExpression(Expression expression)
        {
            return N1QlExpressionTreeVisitor.GetN1QlExpression(expression, _queryGenerationContext);
        }

        private string GetExtentName(IQuerySource querySource)
        {
            return _queryGenerationContext.ExtentNameProvider.GetExtentName(querySource);
        }
    }
}