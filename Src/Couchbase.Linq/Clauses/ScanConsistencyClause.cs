﻿using System;
using System.Linq.Expressions;
using Couchbase.Query;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.Clauses
{
    /// <summary>
    /// Represents a clause on the query setting the scan consistency.
    /// </summary>
    internal class ScanConsistencyClause : IBodyClause
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanConsistencyClause" /> class.
        /// </summary>
        /// <param name="scanConsistency">Scan consistency for the query.</param>
        /// <param name="scanWait">Time to wait for index scan.</param>
        public ScanConsistencyClause(QueryScanConsistency scanConsistency, TimeSpan? scanWait)
        {
            ScanConsistency = scanConsistency;
            ScanWait = scanWait;
        }

        /// <summary>
        /// Scan consistency for the query.
        /// </summary>
        public QueryScanConsistency ScanConsistency { get; set; }

        /// <summary>
        /// Time to wait for index scan.
        /// </summary>
        public TimeSpan? ScanWait { get; }

        /// <inheritdoc />
        public virtual void Accept(IQueryModelVisitor visitor, QueryModel queryModel, int index)
        {
            // Do nothing, the ClusterQueryExecutor will find this in QueryModel.BodyClauses to apply behavior
        }

        /// <inheritdoc />
        public IBodyClause Clone(CloneContext cloneContext)
        {
            return new ScanConsistencyClause(ScanConsistency, ScanWait);
        }

        /// <inheritdoc />
        public virtual void TransformExpressions(Func<Expression, Expression> transformation)
        {
            // Do nothing
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"with scan consistency {ScanConsistency}";
        }
    }
}