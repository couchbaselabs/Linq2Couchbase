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
        public static IEnumerable<MethodInfo> GetSupportedMethods() =>
            new[] {QueryExtensionMethods.ScanConsistency};

        /// <summary>
        /// Creates a new ScanConsistencyExpressionNode.
        /// </summary>
        /// <param name="parseInfo">Method parse info.</param>
        /// <param name="scanConsistency">Scan consistency for the query.</param>
        public ScanConsistencyExpressionNode(MethodCallExpressionParseInfo parseInfo, ConstantExpression scanConsistency)
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

            ScanConsistency = scanConsistency;
        }

        /// <summary>
        /// Scan consistency for the query.
        /// </summary>
        public ConstantExpression ScanConsistency { get; }

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
            queryModel.BodyClauses.Add(new ScanConsistencyClause((QueryScanConsistency) ScanConsistency.Value));
        }
    }
}