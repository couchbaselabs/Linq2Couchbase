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
    /// Expression parser for N1QL "Explain" method.
    /// </summary>
    internal class ExplainExpressionNode : ResultOperatorExpressionNodeBase
    {
        /// <summary>
        /// Methods which are supported by this type of node.
        /// </summary>
        public static IEnumerable<MethodInfo> GetSupportedMethods() => new[]
        {
            QueryExtensionMethods.Explain
        };

        /// <summary>
        /// Creates a new ExplainExpressionNode.
        /// </summary>
        /// <param name="parseInfo">Method parse info.</param>
        public ExplainExpressionNode(MethodCallExpressionParseInfo parseInfo)
            : base(parseInfo, null, null)
        {
        }

        /// <inheritdoc />
        protected override ResultOperatorBase CreateResultOperator(
            ClauseGenerationContext clauseGenerationContext)
        {
            return new ExplainResultOperator();
        }

        /// <inheritdoc />
        public override Expression Resolve(ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            throw new NotSupportedException($"Resolve is not supported by {typeof(ExplainExpressionNode)}");
        }
    }
}
