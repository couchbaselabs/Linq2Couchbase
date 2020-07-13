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
    /// Represents a node on the query setting the scan consistency.
    /// </summary>
    internal class ScanConsistencyExpressionNode : MethodCallExpressionNodeBase
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods() => new[]
        {
            QueryExtensionMethods.ScanConsistency,
            QueryExtensionMethods.ScanConsistencyWithScanWait
        };

        /// <summary>
        /// Creates a new ScanConsistencyExpressionNode.
        /// </summary>
        /// <param name="parseInfo">Method parse info.</param>
        /// <param name="scanConsistency">Scan consistency for the query.</param>
        /// <param name="scanWait">Time to wait for index scan.</param>
        public ScanConsistencyExpressionNode(MethodCallExpressionParseInfo parseInfo, ConstantExpression scanConsistency,
            ConstantExpression scanWait)
            : base(parseInfo)
        {
            if (scanConsistency == null)
            {
                throw new ArgumentNullException(nameof(scanConsistency));
            }
            if (scanConsistency.Type != typeof(QueryScanConsistency))
            {
                throw new ArgumentException($"{nameof(scanConsistency)} must return a {typeof(QueryScanConsistency)}", nameof(scanConsistency));
            }
            if (scanWait != null && scanWait.Type != typeof(TimeSpan))
            {
                throw new ArgumentException($"{nameof(scanWait)} must return a {typeof(TimeSpan)}", nameof(scanWait));
            }

            ScanConsistency = scanConsistency;
            ScanWait = scanWait;
        }

        /// <summary>
        /// Scan consistency for the query.
        /// </summary>
        public ConstantExpression ScanConsistency { get; }

        /// <summary>
        /// Time to wait for index scan.
        /// </summary>
        public ConstantExpression ScanWait { get; }

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
            queryModel.BodyClauses.Add(new ScanConsistencyClause(
                (QueryScanConsistency) ScanConsistency.Value,
                (TimeSpan?) ScanWait?.Value));
        }
    }
}