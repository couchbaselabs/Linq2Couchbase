using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Linq.Execution;

namespace Couchbase.Linq.Extensions
{
    /// <summary>
    /// Extensions to <see cref="IQueryable{T}" /> for use in queries against a <see cref="BucketContext"/>.
    /// </summary>
    public static partial class QueryExtensions
    {
        /// <summary>
        /// Returns an <see cref="IAsyncEnumerable{T}"/> for an <see cref="IQueryable{T}"/> to allow async enumeration.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> which can be enumerated asynchronously.</returns>
        /// <remarks>
        /// This method stops subsequent methods in the chain from being applied to any generated query.
        /// For example, a subsequent call to Queryable.Where to apply a predicate would be applied in-memory
        /// to the query results rather than adding the predicate to the generated query.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The implementation of <see cref="IQueryable{T}"/> does not also implement <see cref="IAsyncEnumerable{T}"/>.</exception>
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> source) =>
            source as IAsyncEnumerable<T> ?? throw new InvalidOperationException("The implementation of IQueryable<T> does not also implement IAsyncEnumerable<T>.");

        /// <summary>
        /// Executes an <see cref="IQueryable{T}"/> asynchronously and returns a list of results.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/></param>.
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/>.</param>
        /// <returns>A list of results.</returns>
        /// <exception cref="InvalidOperationException">The implementation of <see cref="IQueryable{T}"/> does not also implement <see cref="IAsyncEnumerable{T}"/>.</exception>
        public static ValueTask<List<T>> ToListAsync<T>(this IQueryable<T> source,
            CancellationToken cancellationToken = default) =>
            source.AsAsyncEnumerable().ToListAsync(cancellationToken);

        private static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            Expression expression,
            CancellationToken cancellationToken = default)
        {
            if (source.Provider is IAsyncQueryProvider provider)
            {
                if (operatorMethodInfo.IsGenericMethod)
                {
                    operatorMethodInfo
                        = operatorMethodInfo.GetGenericArguments().Length == 2
                            ? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult).GetGenericArguments().Single())
                            : operatorMethodInfo.MakeGenericMethod(typeof(TSource));
                }

                var updatedExpression = Expression.Call(
                    instance: null,
                    method: operatorMethodInfo,
                    arguments: expression == null
                        ? new[] {source.Expression}
                        : new[] {source.Expression, expression});

                return provider.ExecuteAsync<TResult>(updatedExpression, cancellationToken);
            }

            throw new InvalidOperationException("The provided IQueryable is not backed by an IAsyncQueryProvider.");
        }

        private static Expression GetSourceExpression<TSource>(IEnumerable<TSource> source)
        {
            if (source is IQueryable<TSource> q)
            {
                return q.Expression;
            }

            return Expression.Constant(source, typeof(IEnumerable<TSource>));
        }
    }
}