using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Linq.Extensions
{
    // CountAsync and LongCountAsync extensions

    public static partial class QueryExtensions
    {
        /// <summary>
        /// Asynchronously retrieves the number of items returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>The number of items returned by the query.</returns>
        public static Task<int> CountAsync<T>(this IQueryable<T> source) =>
            source.CountAsync(default(CancellationToken));

        /// <summary>
        /// Asynchronously retrieves the number of items returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of items returned by the query.</returns>
        public static Task<int> CountAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<int>>(QueryExtensionMethods.CountAsyncNoPredicate, source, null,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the number of items returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <returns>The number of items returned by the query.</returns>
        public static Task<int> CountAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate) =>
            source.CountAsync(predicate, default);

        /// <summary>
        /// Asynchronously retrieves the number of items returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of items returned by the query.</returns>
        public static Task<int> CountAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ExecuteAsync<T, Task<int>>(QueryExtensionMethods.CountAsyncWithPredicate, source, predicate,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the number of items returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>The number of items returned by the query.</returns>
        public static Task<long> LongCountAsync<T>(this IQueryable<T> source) =>
            source.LongCountAsync(default(CancellationToken));

        /// <summary>
        /// Asynchronously retrieves the number of items returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of items returned by the query.</returns>
        public static Task<long> LongCountAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<long>>(QueryExtensionMethods.LongCountAsyncNoPredicate, source, null,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the number of items returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <returns>The number of items returned by the query.</returns>
        public static Task<long> LongCountAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate) =>
            source.LongCountAsync(predicate, default);

        /// <summary>
        /// Asynchronously retrieves the number of items returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of items returned by the query.</returns>
        public static Task<long> LongCountAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ExecuteAsync<T, Task<long>>(QueryExtensionMethods.LongCountAsyncWithPredicate, source, predicate,
                cancellationToken);
        }
    }
}
