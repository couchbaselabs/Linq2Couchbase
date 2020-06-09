using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Couchbase.Linq.Execution
{
    internal interface IAsyncQueryProvider : IQueryProvider
    {
        T ExecuteAsync<T>(Expression expression, CancellationToken cancellationToken = default);
    }
}
