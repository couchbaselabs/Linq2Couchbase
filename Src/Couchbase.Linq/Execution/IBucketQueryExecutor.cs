using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration;
using Couchbase.N1QL;
using Remotion.Linq;

namespace Couchbase.Linq.Execution
{
    /// <summary>
    /// Extends <see cref="IQueryExecutor"/> with routines to execute a <see cref="LinqQueryRequest"/> asynchronously.
    /// </summary>
    internal interface IBucketQueryExecutor : IQueryExecutor
    {
        /// <summary>
        /// Specifies the consistency guarantee/constraint for index scanning.
        /// </summary>
        ScanConsistency? ScanConsistency { get; set; }

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
        /// Specifies if the query results should be streamed, reducing memory utilzation for large result sets.
        /// </summary>
        /// <remarks>The default is false.</remarks>
        bool UseStreaming { get; set; }

        /// <summary>
        /// Requires that the indexes but up to date with a <see cref="MutationState"/> before the query is executed.
        /// </summary>
        /// <param name="state"><see cref="MutationState"/> used for conistency controls.</param>
        /// <remarks>If called multiple times, the states from the calls are combined.</remarks>
        void ConsistentWith(MutationState state);

        /// <summary>
        /// Asynchronously execute a <see cref="LinqQueryRequest"/>.
        /// </summary>
        /// <typeparam name="T">Type returned by the query.</typeparam>
        /// <param name="queryRequest">Request to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task which contains a list of objects returned by the request when complete.</returns>
        Task<IEnumerable<T>> ExecuteCollectionAsync<T>(LinqQueryRequest queryRequest, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously execute a <see cref="LinqQueryRequest"/> that returns a single result.
        /// </summary>
        /// <typeparam name="T">Type returned by the query.</typeparam>
        /// <param name="queryRequest">Request to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task which contains the object returned by the request when complete.</returns>
        Task<T> ExecuteSingleAsync<T>(LinqQueryRequest queryRequest, CancellationToken cancellationToken);
    }
}
