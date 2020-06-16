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
    /// Expression node for SumAsync.
    /// </summary>
    internal class SumAsyncExpressionNode : ResultOperatorExpressionNodeBase
    {
        /// <summary>
        /// Methods which are supported by this type of node.
        /// </summary>
        public static IEnumerable<MethodInfo> GetSupportedMethods() => new[]
        {
            QueryExtensionMethods.SumAsyncNoSelector,
            QueryExtensionMethods.SumAsyncWithSelector
        };

        /// <summary>
        /// Creates a new SumAsyncExpressionNode.
        /// </summary>
        /// <param name="parseInfo">Method parse info.</param>
        /// <param name="optionalSelector">Optional selector for value to be summed.</param>
        public SumAsyncExpressionNode(MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalSelector)
            : base(parseInfo, null, optionalSelector)
        {
        }

        /// <inheritdoc />
        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            throw new NotSupportedException($"Resolve is not supported by {typeof(SumAsyncExpressionNode)}");
        }

        /// <inheritdoc />
        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext) =>
            new SumAsyncResultOperator();
    }
}
