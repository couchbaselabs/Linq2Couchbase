using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Linq.Extensions
{
    // AnyAsync and AllAsync extensions

    public static partial class QueryExtensions
    {
        /// <summary>
        /// Asynchronously tests if the query returns any results.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>True if the query returns any results..</returns>
        public static Task<bool> AnyAsync<T>(this IQueryable<T> source) =>
            source.AnyAsync(default(CancellationToken));

        /// <summary>
        /// Asynchronously tests if the query returns any results.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the query returns any results..</returns>
        public static Task<bool> AnyAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<bool>>(QueryExtensionMethods.AnyAsyncNoPredicate, source, null,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously tests if the query returns any results.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <returns>True if the query returns any results..</returns>
        public static Task<bool> AnyAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate) =>
            source.AnyAsync(predicate, default);

        /// <summary>
        /// Asynchronously tests if the query returns any results.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the query returns any results..</returns>
        public static Task<bool> AnyAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ExecuteAsync<T, Task<bool>>(QueryExtensionMethods.AnyAsyncWithPredicate, source, predicate,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously tests if all items returned by the query match a predicate.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to test all query results.</param>
        /// <returns>True if all query results match the predicate.</returns>
        public static Task<bool> AllAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate) =>
            source.AllAsync(predicate, default);

        /// <summary>
        /// Asynchronously tests if all items returned by the query match a predicate.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to test all query results.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if all query results match the predicate.</returns>
        public static Task<bool> AllAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ExecuteAsync<T, Task<bool>>(QueryExtensionMethods.AllAsync, source, predicate,
                cancellationToken);
        }
    }
}
