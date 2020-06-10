using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;

namespace Couchbase.Linq.Execution
{
    /// <summary>
    /// Extends <see cref="IQueryExecutor"/> with routines to execute a <see cref="LinqQueryOptions"/> asynchronously.
    /// </summary>
    internal interface IClusterQueryExecutor : IQueryExecutor
    {
        IAsyncEnumerable<T> ExecuteCollectionAsync<T>(QueryModel queryModel, CancellationToken cancellationToken = default);

        Task<T> ExecuteSingleAsync<T>(QueryModel queryModel, bool returnDefaultWhenEmpty, CancellationToken cancellationToken = default);
    }
}
