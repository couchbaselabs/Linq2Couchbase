using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Operators;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Clauses
{
    /// <summary>
    /// Expression node for AnyAsync.
    /// </summary>
    internal class AllAsyncExpressionNode : ResultOperatorExpressionNodeBase
    {
        private readonly ResolvedExpressionCache<Expression> _resolvedPredicateCache;

        public LambdaExpression Predicate { get; }

        /// <summary>
        /// Methods which are supported by this type of node.
        /// </summary>
        public static IEnumerable<MethodInfo> GetSupportedMethods() => new[]
        {
            QueryExtensionMethods.AllAsync
        };

        /// <summary>
        /// Creates a new AllAsyncExpressionNode.
        /// </summary>
        /// <param name="parseInfo">Method parse info.</param>
        /// <param name="predicate">Predicate which filters the results.</param>
        public AllAsyncExpressionNode(MethodCallExpressionParseInfo parseInfo, LambdaExpression predicate)
            : base(parseInfo, null, null)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _resolvedPredicateCache = new ResolvedExpressionCache<Expression>(this);
        }

        private Expression GetResolvedPredicate(ClauseGenerationContext context) =>
            _resolvedPredicateCache.GetOrCreate(
                p => p.GetResolvedExpression(Predicate.Body, Predicate.Parameters[0], context));

        /// <inheritdoc />
        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            throw new NotSupportedException($"Resolve is not supported by {typeof(CountAsyncExpressionNode)}");
        }

        /// <inheritdoc />
        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext) =>
            new AllAsyncResultOperator(GetResolvedPredicate(clauseGenerationContext));
    }
}
