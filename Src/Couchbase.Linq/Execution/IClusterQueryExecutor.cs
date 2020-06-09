﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Query;
using Remotion.Linq;

namespace Couchbase.Linq.Execution
{
    /// <summary>
    /// Extends <see cref="IQueryExecutor"/> with routines to execute a <see cref="LinqQueryOptions"/> asynchronously.
    /// </summary>
    internal interface IClusterQueryExecutor : IQueryExecutor
    {
        /// <summary>
        /// Specifies the consistency guarantee/constraint for index scanning.
        /// </summary>
        QueryScanConsistency? ScanConsistency { get; set; }

        /// <summary>
        /// Specifies the maximum time the client is willing to wait for an index to catch up to the consistency requirement in the request.
        /// If an index has to catch up, and the time is exceed doing so, an error is returned.
        /// </summary>
        TimeSpan? ScanWait { get; set; }

        /// <summary>
        /// Specifies the maximum time the server should wait for the QueryRequest to execute.
        /// </summary>
        TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Requires that the indexes but up to date with a <see cref="MutationState"/> before the query is executed.
        /// </summary>
        /// <param name="state"><see cref="MutationState"/> used for consistency controls.</param>
        /// <remarks>If called multiple times, the states from the calls are combined.</remarks>
        void ConsistentWith(MutationState state);

        IAsyncEnumerable<T> ExecuteCollectionAsync<T>(QueryModel queryModel, CancellationToken cancellationToken = default);

        Task<T> ExecuteSingleAsync<T>(QueryModel queryModel, bool returnDefaultWhenEmpty, CancellationToken cancellationToken = default);
    }
}
