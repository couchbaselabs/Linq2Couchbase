using System;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Linq.QueryGeneration.FromParts;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Processes the predicate of a <see cref="WhereClause"/> to find .Any() subqueries
    /// against group join (NEST) extents.  If found, converts the group join from a
    /// LEFT NEST to an INNER NEST and drops the subquery from the predicate.
    /// </summary>
    internal class InnerNestDetectingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly QueryPartsAggregator _queyPartsAggregator;

        /// <summary>
        /// Creates a new InnerNestDetectingExpressionVisitor.
        /// </summary>
        /// <param name="queyPartsAggregator"><see cref="QueryPartsAggregator"/> for the current query.</param>
        public InnerNestDetectingExpressionVisitor(QueryPartsAggregator queyPartsAggregator)
        {
            _queyPartsAggregator = queyPartsAggregator ?? throw new ArgumentNullException(nameof(queyPartsAggregator));
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)
            {
                var left = Visit(node.Left);
                var right = Visit(node.Right);

                if (left != null && right != null)
                {
                    if (left.Equals(node.Left) && right.Equals(node.Right))
                    {
                        // Was not modified
                        return node;
                    }
                    else
                    {
                        // One of the sides was modified, so build a replacement binary expression
                        return Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull,
                            node.Method, node.Conversion);
                    }
                }
                else if (left != null)
                {
                    // Right side was dropped
                    return left;
                }
                else
                {
                    // Left side or both sides were dropped
                    return right;
                }
            }
            else
            {
                // Don't recurse into anything other than &&
                return node;
            }
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var queryModel = expression.QueryModel;

            if (!IsSimpleAnySubqueryAgainstGroupJoin(queryModel, out var groupJoinClause))
            {
                // We don't care about subqueries with any body clauses
                return expression;
            }

            // See if this group join is a LEFT OUTER NEST
            var ansiJoinPart = _queyPartsAggregator.Extents.OfType<AnsiJoinPart>()
                .FirstOrDefault(p => p.QuerySource == groupJoinClause);

            if (ansiJoinPart != null && ansiJoinPart.JoinType == JoinTypes.LeftNest)
            {
                // Convert from a LEFT OUTER NEST to an INNER NEST
                ansiJoinPart.JoinType = JoinTypes.InnerNest;

                // And drop the expression from the WHERE clause
                return null;
            }
            else
            {
                return expression;
            }
        }

        private static bool IsSimpleAnySubqueryAgainstGroupJoin(QueryModel queryModel, out GroupJoinClause groupJoinClause)
        {
            groupJoinClause = null;

            if (queryModel.BodyClauses.Count > 0)
            {
                return false;
            }

            if (queryModel.ResultOperators.Count != 1)
            {
                return false;
            }

            if (!(queryModel.ResultOperators[0] is AnyResultOperator))
            {
                return false;
            }

            if (queryModel.MainFromClause.FromExpression is QuerySourceReferenceExpression referenceExpression)
            {
                groupJoinClause = referenceExpression.ReferencedQuerySource as GroupJoinClause;

                return groupJoinClause != null;
            }

            return false;
        }
    }
}
