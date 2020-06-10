using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Linq.Extensions
{
    // FirstAsync and FirstOrDefaultAsync extensions

    public static partial class QueryExtensions
    {
        /// <summary>
        /// Asynchronously retrieves the first item returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>The first item returned by the query.</returns>
        /// <exception cref="InvalidOperationException">The query returns no results.</exception>
        public static Task<T> FirstAsync<T>(this IQueryable<T> source) =>
            source.FirstAsync<T>(default(CancellationToken));

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The first item returned by the query.</returns>
        /// <exception cref="InvalidOperationException">The query returns no results.</exception>
        public static Task<T> FirstAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.FirstAsyncNoPredicate, source, null,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query, filtered by a predicate.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <returns>The first item returned by the query.</returns>
        /// <exception cref="InvalidOperationException">The query returns no results.</exception>
        public static Task<T> FirstAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate) =>
            source.FirstAsync<T>(predicate, default);

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query, filtered by a predicate.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The first item returned by the query.</returns>
        /// <exception cref="InvalidOperationException">The query returns no results.</exception>
        public static Task<T> FirstAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.FirstAsyncWithPredicate, source, predicate,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>The first item returned by the query, or the default value of <typeparamref name="T"/> if empty.</returns>
        public static Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source) =>
            source.FirstOrDefaultAsync<T>(default(CancellationToken));

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The first item returned by the query, or the default value of <typeparamref name="T"/> if empty.</returns>
        public static Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.FirstOrDefaultAsyncNoPredicate, source, null,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query, filtered by a predicate.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <returns>The first item returned by the query, or the default value of <typeparamref name="T"/> if empty.</returns>
        public static Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate) =>
            source.FirstOrDefaultAsync<T>(predicate, default);

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query, filtered by a predicate.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The first item returned by the query, or the default value of <typeparamref name="T"/> if empty.</returns>
        public static Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.FirstOrDefaultAsyncWithPredicate, source, predicate,
                cancellationToken);
        }
    }
}
