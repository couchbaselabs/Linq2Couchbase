using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Linq.Extensions
{
    // MinAsync and MaxAsync extensions

    public static partial class QueryExtensions
    {
        /// <summary>
        /// Asynchronously finds the minimum item returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>The minimum item returned by the query.</returns>
        public static Task<T> MinAsync<T>(this IQueryable<T> source) =>
            source.MinAsync(default(CancellationToken));

        /// <summary>
        /// Asynchronously finds the minimum item returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The minimum item returned by the query.</returns>
        public static Task<T> MinAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.MinAsyncNoSelector, source, null,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously finds the minimum item returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <typeparam name="TResult">Type returned by the minimum operation.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="selector">Selector for value.</param>
        /// <returns>The minimum item returned by the query.</returns>
        public static Task<TResult> MinAsync<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector) =>
            source.MinAsync(selector, default);

        /// <summary>
        /// Asynchronously finds the minimum item returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <typeparam name="TResult">Type returned by the minimum operation.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="selector">Selector for value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The minimum item returned by the query.</returns>
        public static Task<TResult> MinAsync<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return ExecuteAsync<T, Task<TResult>>(QueryExtensionMethods.MinAsyncWithSelector, source, selector,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously finds the maximum item returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>The maximum item returned by the query.</returns>
        public static Task<T> MaxAsync<T>(this IQueryable<T> source) =>
            source.MaxAsync(default(CancellationToken));

        /// <summary>
        /// Asynchronously finds the maximum item returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The maximum item returned by the query.</returns>
        public static Task<T> MaxAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.MaxAsyncNoSelector, source, null,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously finds the maximum item returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <typeparam name="TResult">Type returned by the maximum operation.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="selector">Selector for value.</param>
        /// <returns>The maximum item returned by the query.</returns>
        public static Task<TResult> MaxAsync<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector) =>
            source.MaxAsync(selector, default);

        /// <summary>
        /// Asynchronously finds the maximum item returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <typeparam name="TResult">Type returned by the maximum operation.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="selector">Selector for value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The maximum item returned by the query.</returns>
        public static Task<TResult> MaxAsync<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return ExecuteAsync<T, Task<TResult>>(QueryExtensionMethods.MaxAsyncWithSelector, source, selector,
                cancellationToken);
        }
    }
}
