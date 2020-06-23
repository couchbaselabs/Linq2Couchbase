using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Linq.Extensions
{
    // AverageAsync extensions

    public static partial class QueryExtensions
    {
        /// <summary>
        /// Asynchronously averages the items returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>The average of the items returned by the query.</returns>
        public static Task<T> AverageAsync<T>(this IQueryable<T> source) =>
            source.AverageAsync(default(CancellationToken));

        /// <summary>
        /// Asynchronously averages the items returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The average of the items returned by the query.</returns>
        public static Task<T> AverageAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<T>>(QueryExtensionMethods.AverageAsyncNoSelector, source, null,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously averages the items returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <typeparam name="TResult">Type returned by the sum operation.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="selector">Selector for value to be summed.</param>
        /// <returns>The average of the items returned by the query.</returns>
        public static Task<TResult> AverageAsync<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector) =>
            source.AverageAsync(selector, default);

        /// <summary>
        /// Asynchronously averages the items returned by a query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <typeparam name="TResult">Type returned by the sum operation.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="selector">Selector for value to be summed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The average of the items returned by the query.</returns>
        public static Task<TResult> AverageAsync<T, TResult>(this IQueryable<T> source, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return ExecuteAsync<T, Task<TResult>>(QueryExtensionMethods.AverageAsyncWithSelector, source, selector,
                cancellationToken);
        }
    }
}
