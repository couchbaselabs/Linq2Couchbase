using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using Couchbase.Linq.Execution;
using Remotion.Linq;

namespace Couchbase.Linq
{
    /// <summary>
    /// The executor of the query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CouchbaseQueryable<T> : QueryableBase<T>, IAsyncEnumerable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CouchbaseQueryable{T}"/> class.
        /// </summary>
        /// <remarks>Used to build new expressions as more methods are applied to the query.</remarks>
        /// <param name="provider">The provider.</param>
        /// <param name="expression">The expression.</param>
        public CouchbaseQueryable(IAsyncQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CouchbaseQueryable{T}"/> class.
        /// </summary>
        /// <remarks>Used by subclasses to create a root queryable.</remarks>
        /// <param name="provider">The provider.</param>
        protected CouchbaseQueryable(IAsyncQueryProvider provider)
            : base(provider)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            ((IAsyncQueryProvider) Provider).ExecuteAsync<IAsyncEnumerable<T>>(Expression)
                .GetAsyncEnumerator(cancellationToken);
    }
}