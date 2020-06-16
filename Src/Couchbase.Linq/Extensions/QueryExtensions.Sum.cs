using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Linq.Extensions
{
    // SumAsync extensions

    public static partial class QueryExtensions
    {
        /// <summary>
        /// Asynchronously sums the items returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>The sum of the items returned by the query.</returns>
        public static Task<T> SumAsync<T>(this IQueryable<T> source) =>
            source.SumAsync(default(CancellationToken));

        /// <summary>
        /// Asynchronously sums the items returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The sum of the items returned by the query.</returns>
        public static Task<T> SumAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.SumAsyncNoSelector, source, null,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously sums the items returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <typeparam name="TResult">Type returned by the sum operation.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="selector">Selector for value to be summed.</param>
        /// <returns>The sum of the items returned by the query.</returns>
        public static Task<TResult> SumAsync<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector) =>
            source.SumAsync(selector, default);

        /// <summary>
        /// Asynchronously sums the items returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <typeparam name="TResult">Type returned by the sum operation.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="selector">Selector for value to be summed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The sum of the items returned by the query.</returns>
        public static Task<TResult> SumAsync<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return ExecuteAsync<T, Task<TResult>>(QueryExtensionMethods.SumAsyncWithSelector, source, selector,
                cancellationToken);
        }
    }
}
