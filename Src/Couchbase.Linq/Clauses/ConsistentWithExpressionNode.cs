using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.Extensions;
using Couchbase.Query;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Couchbase.Linq.Clauses
{
    /// <summary>
    /// Represents a node on the query setting index consistency using <see cref="MutationState"/>.
    /// If applied multiple times to the same query, the states should be merged.
    /// </summary>
    internal class ConsistentWithExpressionNode : MethodCallExpressionNodeBase
    {
        /// <summary>
        /// Methods which are supported by this type of node.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetSupportedMethods() => new[]
        {
            QueryExtensionMethods.ConsistentWith,
            QueryExtensionMethods.ConsistentWithScanWait
        };

        /// <summary>
        /// Creates a new ConsistentWithExpressionNode.
        /// </summary>
        /// <param name="parseInfo">Method parse info.</param>
        /// <param name="mutationState">Mutation state for the query.</param>
        /// <param name="scanWait">Time to wait for index scan.</param>
        public ConsistentWithExpressionNode(MethodCallExpressionParseInfo parseInfo, ConstantExpression mutationState,
            ConstantExpression? scanWait)
            : base(parseInfo)
        {
            if (mutationState == null)
            {
                throw new ArgumentNullException(nameof(mutationState));
            }
            if (mutationState.Type != typeof(MutationState))
            {
                throw new ArgumentException($"{nameof(mutationState)} must return a {typeof(MutationState)}", nameof(mutationState));
            }
            if (scanWait != null && scanWait.Type != typeof(TimeSpan))
            {
                throw new ArgumentException($"{nameof(scanWait)} must return a {typeof(TimeSpan)}", nameof(scanWait));
            }

            MutationState = mutationState;
            ScanWait = scanWait;
        }

        /// <summary>
        /// Mutation state for the query.
        /// </summary>
        public ConstantExpression MutationState { get; }

        /// <summary>
        /// Time to wait for index scan.
        /// </summary>
        public ConstantExpression? ScanWait { get; }

        /// <inheritdoc />
        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        /// <inheritdoc />
        protected override void ApplyNodeSpecificSemantics(QueryModel queryModel,
            ClauseGenerationContext clauseGenerationContext)
        {
            queryModel.BodyClauses.Add(new ConsistentWithClause(
                (MutationState) MutationState.Value!,
                (TimeSpan?) ScanWait?.Value));
        }
    }
}