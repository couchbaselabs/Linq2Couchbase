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
    public class N1QlQueryModelVisitor : QueryModelVisitorBase //: N1QlQueryModelVisitorBase
    {
        private readonly ParameterAggregator _parameterAggregator = new ParameterAggregator();
        private readonly QueryPartsAggregator _queryPartsAggregator = new QueryPartsAggregator();

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
            queryModel.SelectClause.Accept(this, queryModel);
            queryModel.MainFromClause.Accept(this, queryModel);
            VisitBodyClauses(queryModel.BodyClauses, queryModel);
            VisitResultOperators(queryModel.ResultOperators, queryModel);
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            var bucketConstantExpression = fromClause.FromExpression as ConstantExpression;
            if ((bucketConstantExpression != null) &&
                typeof(IBucketQueryable).IsAssignableFrom(bucketConstantExpression.Type))
            {
                _queryPartsAggregator.AddFromPart(new N1QlFromQueryPart()
                {
                    Source = EscapeIdentifier(((IBucketQueryable) bucketConstantExpression.Value).BucketName),
                    ItemName = EscapeIdentifier(fromClause.ItemName)
                });
            }
            else if (fromClause.FromExpression.NodeType == ExpressionType.MemberAccess)
            {
                _queryPartsAggregator.AddFromPart(new N1QlFromQueryPart()
                {
                    Source = GetN1QlExpression((MemberExpression) fromClause.FromExpression),
                    ItemName = EscapeIdentifier(fromClause.ItemName)
                });
            }

            base.VisitMainFromClause(fromClause, queryModel);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            foreach (var parameter in GetSelectParameters(selectClause, queryModel))
            {
                _queryPartsAggregator.AddSelectParts(parameter);
            }
            base.VisitSelectClause(selectClause, queryModel);
        }

        private IEnumerable<string> GetSelectParameters(SelectClause selectClause, QueryModel queryModel)
        {
            var prefix = EscapeIdentifier(queryModel.MainFromClause.ItemName);

            var expression = GetN1QlExpression(selectClause.Selector);

            if (selectClause.Selector.GetType() == typeof (QuerySourceReferenceExpression))
            {
                expression = string.Concat(expression, ".*");
            }

            return expression.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
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
            else if (resultOperator is MetaResultOperator)
            {
                _queryPartsAggregator.MetaPart = string.Format("META({0})", EscapeIdentifier(queryModel.MainFromClause.ItemName));
            }
            else if (resultOperator is AnyResultOperator)
            {
                _queryPartsAggregator.QueryType = N1QlQueryType.Any;
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

        //TODO: Implement Joins
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel,
            GroupJoinClause groupJoinClause)
        {
            base.VisitJoinClause(joinClause, queryModel, groupJoinClause);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            _queryPartsAggregator.AddFromPart(new N1QlFromQueryPart()
            {
                Source = joinClause.ItemType.Name.ToLower(),
                ItemName = joinClause.ItemName
            });

            _queryPartsAggregator.AddWherePart("ON KEYS ARRAY {0} FOR {1} IN {2} END",
                joinClause.OuterKeySelector,
                joinClause.InnerKeySelector,
                joinClause.ItemName);

            base.VisitJoinClause(joinClause, queryModel, index);
        }

        private string GetN1QlExpression(Expression expression)
        {
            return N1QlExpressionTreeVisitor.GetN1QlExpression(expression, _parameterAggregator);
        }

        /// <summary>
        ///     Ensures that if the identifier contains a hyphen or other special characters that it will be escaped by tick (`) characters.
        /// </summary>
        /// <param name="identifier">The identifier to format</param>
        /// <returns>An escaped identifier, if escaping was required.  Otherwise the original identifier.</returns>
        public static string EscapeIdentifier(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            bool containsSpecialChar = false;
            for (var i = 0; i < identifier.Length; i++)
            {
                if (!Char.IsLetterOrDigit(identifier[i]))
                {
                    containsSpecialChar = true;
                    break;
                }
            }

            if (!containsSpecialChar)
            {
                return identifier;
            }
            else
            {
                var sb = new System.Text.StringBuilder(identifier.Length + 2);

                sb.Append('`');
                sb.Append(identifier.Replace("`", "``"));
                sb.Append('`');
                return sb.ToString();
            }
        }
    }
}