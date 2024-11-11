using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Remotion.Linq;

namespace Couchbase.Linq.Execution
{
    /// <summary>
    /// Extends <see cref="IQueryExecutor"/> with routines to execute queries asynchronously.
    /// </summary>
    internal interface IAsyncQueryExecutor : IQueryExecutor
    {
        IAsyncEnumerable<T> ExecuteCollectionAsync<T>(QueryModel queryModel, CancellationToken cancellationToken = default);

        Task<T?> ExecuteSingleAsync<T>(QueryModel queryModel, bool returnDefaultWhenEmpty, CancellationToken cancellationToken = default);

        Task<T> ExecuteScalarAsync<T>(QueryModel queryModel, CancellationToken cancellationToken = default);
    }
}
