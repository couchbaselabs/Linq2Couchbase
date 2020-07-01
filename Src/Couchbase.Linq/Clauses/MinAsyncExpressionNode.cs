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
    /// Expression node for MinAsync.
    /// </summary>
    internal class MinAsyncExpressionNode : ResultOperatorExpressionNodeBase
    {
        /// <summary>
        /// Methods which are supported by this type of node.
        /// </summary>
        public static IEnumerable<MethodInfo> GetSupportedMethods() => new[]
        {
            QueryExtensionMethods.MinAsyncNoSelector,
            QueryExtensionMethods.MinAsyncWithSelector
        };

        /// <summary>
        /// Creates a new MinAsyncExpressionNode.
        /// </summary>
        /// <param name="parseInfo">Method parse info.</param>
        /// <param name="optionalSelector">Optional selector for value.</param>
        public MinAsyncExpressionNode(MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalSelector)
            : base(parseInfo, null, optionalSelector)
        {
        }

        /// <inheritdoc />
        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            throw new NotSupportedException($"Resolve is not supported by {typeof(MinAsyncExpressionNode)}");
        }

        /// <inheritdoc />
        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext) =>
            new MinAsyncResultOperator();
    }
}
