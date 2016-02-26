using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;

namespace Couchbase.Linq.Execution
{
    /// <summary>
    /// Extends <see cref="IQueryExecutor"/> with routines to execute a <see cref="LinqQueryRequest"/> asynchronously.
    /// </summary>
    internal interface IBucketQueryExecutor : IQueryExecutor
    {
        /// <summary>
        /// Asynchronously execute a <see cref="LinqQueryRequest"/>.
        /// </summary>
        /// <typeparam name="T">Type returned by the query.</typeparam>
        /// <param name="queryRequest">Request to execute.</param>
        /// <returns>Task which contains a list of objects returned by the request when complete.</returns>
        Task<IEnumerable<T>> ExecuteCollectionAsync<T>(LinqQueryRequest queryRequest);

        /// <summary>
        /// Asynchronously execute a <see cref="LinqQueryRequest"/> that returns a single result.
        /// </summary>
        /// <typeparam name="T">Type returned by the query.</typeparam>
        /// <param name="queryRequest">Request to execute.</param>
        /// <returns>Task which contains the object returned by the request when complete.</returns>
        Task<T> ExecuteSingleAsync<T>(LinqQueryRequest queryRequest);
    }
}
