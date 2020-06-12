using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Linq.Extensions
{
    // SingleAsync and SingleOrDefaultAsync extensions

    public static partial class QueryExtensions
    {
        /// <summary>
        /// Asynchronously retrieves the first item returned by the query. Throws an exception if more than one item is returned.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>The item returned by the query.</returns>
        /// <exception cref="InvalidOperationException">The query returns no results or more than one result.</exception>
        public static Task<T> SingleAsync<T>(this IQueryable<T> source) =>
            source.SingleAsync<T>(default(CancellationToken));

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query. Throws an exception if more than one item is returned.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The item returned by the query.</returns>
        /// <exception cref="InvalidOperationException">The query returns no results or more than one result.</exception>
        public static Task<T> SingleAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.SingleAsyncNoPredicate, source, null,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query, filtered by a predicate. Throws an exception if more than one item is returned.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <returns>The item returned by the query.</returns>
        /// <exception cref="InvalidOperationException">The query returns no results or more than one result.</exception>
        public static Task<T> SingleAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate) =>
            source.SingleAsync<T>(predicate, default);

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query, filtered by a predicate. Throws an exception if more than one item is returned.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The item returned by the query.</returns>
        /// <exception cref="InvalidOperationException">The query returns no results or more than one result.</exception>
        public static Task<T> SingleAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.SingleAsyncWithPredicate, source, predicate,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query. Throws an exception if more than one item is returned.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>The item returned by the query, or the default value of <typeparamref name="T"/> if empty.</returns>
        /// <exception cref="InvalidOperationException">The query returns more than one result.</exception>
        public static Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> source) =>
            source.SingleOrDefaultAsync<T>(default(CancellationToken));

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query. Throws an exception if more than one item is returned.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The item returned by the query, or the default value of <typeparamref name="T"/> if empty.</returns>
        /// <exception cref="InvalidOperationException">The query returns more than one result.</exception>
        public static Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.SingleOrDefaultAsyncNoPredicate, source, null,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query, filtered by a predicate. Throws an exception if more than one item is returned.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <returns>The item returned by the query, or the default value of <typeparamref name="T"/> if empty.</returns>
        /// <exception cref="InvalidOperationException">The query returns more than one result.</exception>
        public static Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate) =>
            source.SingleOrDefaultAsync<T>(predicate, default);

        /// <summary>
        /// Asynchronously retrieves the first item returned by the query, filtered by a predicate. Throws an exception if more than one item is returned.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="predicate">Predicate to filter query results.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The item returned by the query, or the default value of <typeparamref name="T"/> if empty.</returns>
        /// <exception cref="InvalidOperationException">The query returns more than one result.</exception>
        public static Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.SingleOrDefaultAsyncWithPredicate, source, predicate,
                cancellationToken);
        }
    }
}
