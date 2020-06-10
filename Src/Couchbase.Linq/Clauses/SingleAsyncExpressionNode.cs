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
    /// Expression node for FirstAsync and FirstOrDefaultAsync.
    /// </summary>
    internal class SingleAsyncExpressionNode : ResultOperatorExpressionNodeBase
    {
        /// <summary>
        /// Methods which are supported by this type of node.
        /// </summary>
        public static IEnumerable<MethodInfo> GetSupportedMethods() => new[]
        {
            QueryExtensionMethods.SingleAsyncNoPredicate,
            QueryExtensionMethods.SingleAsyncWithPredicate,
            QueryExtensionMethods.SingleOrDefaultAsyncNoPredicate,
            QueryExtensionMethods.SingleOrDefaultAsyncWithPredicate
        };

        /// <summary>
        /// Creates a new SingleAsyncExpressionNode.
        /// </summary>
        /// <param name="parseInfo">Method parse info.</param>
        /// <param name="optionalPredicate">Optional predicate which filters the results.</param>
        public SingleAsyncExpressionNode(MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalPredicate)
            : base(parseInfo, optionalPredicate, null)
        {
        }

        /// <inheritdoc />
        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            throw new NotSupportedException($"Resolve is not supported by {typeof(SingleAsyncExpressionNode)}");
        }

        /// <inheritdoc />
        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext) =>
            new SingleAsyncResultOperator(ParsedExpression.Method.Name.EndsWith("OrDefaultAsync"));
    }
}
