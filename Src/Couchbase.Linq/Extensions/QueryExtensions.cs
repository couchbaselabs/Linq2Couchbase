using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Couchbase.Linq.Execution;
using Couchbase.Linq.Metadata;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Couchbase.Linq.Extensions
{
    /// <summary>
    /// Extentions to <see cref="IQueryable{T}" /> for use in queries against a <see cref="BucketContext"/>.
    /// </summary>
    public static class QueryExtensions
    {
        #region Nest

        /// <summary>
        ///     Nest for N1QL. (outer.Nest(inner, keySelector, resultSelector) translates to NEST inner ON KEYS outer.keySelector)
        /// </summary>
        /// <typeparam name="TOuter">Type of the source sequence</typeparam>
        /// <typeparam name="TInner">Type of the inner sequence being nested</typeparam>
        /// <typeparam name="TResult">Type of the result sequence</typeparam>
        /// <param name="outer"></param>
        /// <param name="inner">Sequence to be nested</param>
        /// <param name="keySelector">Expression to get the list of keys to nest for an item in the source sequence.  Should return a list of strings.</param>
        /// <param name="resultSelector">Expression that returns the result</param>
        /// <remarks>Returns a result for values in the outer sequence only if matching values in the inner sequence are found</remarks>
        /// <returns>Modified IQueryable</returns>
        public static IQueryable<TResult> Nest<TOuter, TInner, TResult>(
            this IQueryable<TOuter> outer,
            IEnumerable<TInner> inner,
            Expression<Func<TOuter, IEnumerable<string>>> keySelector,
            Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector)
        {
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }
            if (outer == null)
            {
                throw new ArgumentNullException("outer");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }

            if (outer is EnumerableQuery<TOuter>)
            {
                // If outer is an IEnumerable converted to IQueryable via AsQueryable
                // Then we need to just call the IEnumerable implementation

                if (!typeof (IDocumentMetadataProvider).IsAssignableFrom(typeof (TInner)))
                {
                    throw new NotSupportedException("Inner Sequence Items Must Implement IDocumentMetadataProvider To Function With EnumerableQuery<T>");
                }

                var methodCall =
                    Expression.Call(
                        typeof(EnumerableExtensions).GetMethod("Nest")
                            .MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TResult)),
                        Expression.Constant(outer, typeof(IEnumerable<TOuter>)),
                        Expression.Constant(inner, typeof(IEnumerable<TInner>)),
                        keySelector,
                        resultSelector);

                return outer.Provider.CreateQuery<TResult>(
                    Expression.Call(
                        typeof(Queryable).GetMethods().First(p => p.Name == "AsQueryable" && p.GetGenericArguments().Length == 1)
                            .MakeGenericMethod(typeof(TResult)),
                        methodCall));
            }
            else
            {
                return outer.Provider.CreateQuery<TResult>(
                    Expression.Call(
                        ((MethodInfo)MethodBase.GetCurrentMethod())
                            .MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TResult)),
                        outer.Expression,
                        GetSourceExpression(inner),
                        Expression.Quote(keySelector),
                        Expression.Quote(resultSelector)));
            }
        }

        /// <summary>
        ///     Nest for N1QL. (outer.LeftNest(inner, keySelector, resultSelector) translates to LEFT OUTER NEST inner ON KEYS outer.keySelector)
        /// </summary>
        /// <typeparam name="TOuter">Type of the source sequence</typeparam>
        /// <typeparam name="TInner">Type of the inner sequence being nested</typeparam>
        /// <typeparam name="TResult">Type of the result sequence</typeparam>
        /// <param name="outer"></param>
        /// <param name="inner">Sequence to be nested</param>
        /// <param name="keySelector">Expression to get the list of keys to nest for an item in the source sequence.  Should return a list of strings.</param>
        /// <param name="resultSelector">Expression that returns the result</param>
        /// <remarks>Returns a result for all values in the outer sequence</remarks>
        /// <returns>Modified IQueryable</returns>
        public static IQueryable<TResult> LeftOuterNest<TOuter, TInner, TResult>(
            this IQueryable<TOuter> outer,
            IEnumerable<TInner> inner,
            Expression<Func<TOuter, IEnumerable<string>>> keySelector,
            Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector)
        {
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }
            if (outer == null)
            {
                throw new ArgumentNullException("outer");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }

            if (outer is EnumerableQuery<TOuter>)
            {
                // If outer is an IEnumerable converted to IQueryable via AsQueryable
                // Then we need to just call the IEnumerable implementation

                if (!typeof(IDocumentMetadataProvider).IsAssignableFrom(typeof(TInner)))
                {
                    throw new NotSupportedException("Inner Sequence Items Must Implement IDocumentMetadataProvider To Function With EnumerableQuery<T>");
                }

                var methodCall =
                    Expression.Call(
                        typeof(EnumerableExtensions).GetMethod("LeftOuterNest")
                            .MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TResult)),
                        Expression.Constant(outer, typeof(IEnumerable<TOuter>)),
                        Expression.Constant(inner, typeof(IEnumerable<TInner>)),
                        keySelector,
                        resultSelector);

                return outer.Provider.CreateQuery<TResult>(
                    Expression.Call(
                        typeof(Queryable).GetMethods().First(p => p.Name == "AsQueryable" && p.GetGenericArguments().Length == 1)
                            .MakeGenericMethod(typeof(TResult)),
                        methodCall));
            }
            else
            {
                return outer.Provider.CreateQuery<TResult>(
                    Expression.Call(
                        ((MethodInfo)MethodBase.GetCurrentMethod())
                            .MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TResult)),
                        outer.Expression,
                        GetSourceExpression(inner),
                        Expression.Quote(keySelector),
                        Expression.Quote(resultSelector)));
            }
        }

        #endregion

        #region UseKeys

        /// <summary>
        ///     Filters documents based on a list of keys
        /// </summary>
        /// <typeparam name="T">Type of the items being filtered</typeparam>
        /// <param name="items">Items being filtered</param>
        /// <param name="keys">Keys to be selected</param>
        /// <returns>Modified IQueryable</returns>
        public static IQueryable<T> UseKeys<T>(
            this IQueryable<T> items,
            IEnumerable<string> keys)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            if (items is EnumerableQuery<T>)
            {
                // If outer is an IEnumerable converted to IQueryable via AsQueryable
                // Then we need to just call the IEnumerable implementation

                if (!typeof(IDocumentMetadataProvider).IsAssignableFrom(typeof(T)))
                {
                    throw new NotSupportedException("Items Sequence Must Implement IDocumentMetadataProvider To Function With EnumerableQuery<T>");
                }

                var methodCall =
                    Expression.Call(
                        typeof(EnumerableExtensions).GetMethod("UseKeys")
                            .MakeGenericMethod(typeof(T)),
                        Expression.Constant(items, typeof(IEnumerable<T>)),
                        Expression.Constant(keys, typeof(IEnumerable<string>)));

                return items.Provider.CreateQuery<T>(
                    Expression.Call(
                        typeof(Queryable).GetMethods().First(p => p.Name == "AsQueryable" && p.GetGenericArguments().Length == 1)
                            .MakeGenericMethod(typeof(T)),
                        methodCall));
            }
            else
            {
                return items.Provider.CreateQuery<T>(
                    Expression.Call(
                        ((MethodInfo)MethodBase.GetCurrentMethod())
                            .MakeGenericMethod(typeof(T)),
                        items.Expression,
                        GetSourceExpression(keys)));
            }
        }

        #endregion

        /// <summary>
        /// The EXPLAIN statement is used before any N1QL statement to obtain information about how the statement operates.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>Explanation of the query</returns>
        public static dynamic Explain<T>(this IQueryable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var newExpression = Expression.Call(null,
                ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (T)),
                source.Expression);

            return source.Provider.Execute<dynamic>(newExpression);
        }

        #region Async

        /// <summary>
        /// Execute a Couchbase query asynchronously.
        /// </summary>
        /// <typeparam name="T">Type being queried.</typeparam>
        /// <param name="source">Query to execute asynchronously.  Must be a Couchbase LINQ query.</param>
        /// <returns>Task which contains the query result when completed.</returns>
        /// <example>
        /// var results = await query.ExecuteAsync();
        /// </example>
        public static async Task<IEnumerable<T>> ExecuteAsync<T>(this IQueryable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!(source is IBucketQueryable) || !(source is IBucketQueryExecutorProvider))
            {
                throw new ArgumentException("ExecuteAsync is only supported on Couchbase LINQ queries.", "source");
            }

            var queryRequest = LinqQueryRequest.CreateQueryRequest(source);

            return await
                ((IBucketQueryExecutorProvider)source).BucketQueryExecutor.ExecuteCollectionAsync<T>(queryRequest)
                    .ConfigureAwait(false);
        }

        /// <summary>
        /// Execute a Couchbase query asynchronously.
        /// </summary>
        /// <typeparam name="T">Type being queried.</typeparam>
        /// <typeparam name="TResult">Type returned by <paramref name="additionalExpression"/>.</typeparam>
        /// <param name="source">Query to execute asynchronously.  Must be a Couchbase LINQ query.</param>
        /// <param name="additionalExpression">Additional expressions to apply to the query before executing.  Typically used for aggregates.</param>
        /// <returns>Task which contains the query result when completed.</returns>
        /// <remarks>
        /// <para>The expression contained in <paramref name="additionalExpression"/> is applied to the query before
        /// it is executed asynchrounously.  Typically, this would be used to apply an aggregate, First, Single,
        /// or other operation to the query that normall  triggers immediate query execution.  Passing these actions
        /// in <paramref name="additionalExpression"/> delays their execution so that they can be handled asynchronously.</para>
        /// <para><paramref name="additionalExpression"/> must return a scalar value or a single object.  It should not return another
        /// instance of <see cref="IQueryable{T}"/>.</para>
        /// </remarks>
        /// <example>
        /// var document = await query.ExecuteAsync(query => query.First());
        /// </example>
        /// <example>
        /// var avg = await query.ExecuteAsync(query => query.Average(p => p.Abv));
        /// </example>
        public static async Task<TResult> ExecuteAsync<T, TResult>(this IQueryable<T> source,
            Expression<Func<IQueryable<T>, TResult>> additionalExpression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!(source is IBucketQueryable) || !(source is IBucketQueryExecutorProvider))
            {
                throw new ArgumentException("ExecuteAsync is only supported on Couchbase LINQ queries.", "source");
            }
            if (typeof (TResult).IsGenericTypeDefinition &&
                (typeof (TResult).GetGenericTypeDefinition() == typeof (IQueryable<>)))
            {
                throw new ArgumentException("additionalExpression must return a scalar value, not IQueryable.", "additionalExpression");
            }

            var queryRequest = LinqQueryRequest.CreateQueryRequest(source, additionalExpression);

            return await
                ((IBucketQueryExecutorProvider)source).BucketQueryExecutor.ExecuteSingleAsync<TResult>(queryRequest)
                    .ConfigureAwait(false);
        }

#endregion

        /// <summary>
        /// An expression generation helper for adding additional methods to a Linq provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TR">The type of the return value.</typeparam>
        /// <param name="source">The <see cref="IQueryable"/> source.</param>
        /// <param name="expression">The expression.</param>
        /// <remarks>Original work from: https://www.re-motion.org/blogs/mix/2010/10/28/re-linq-extensibility-custom-query-operators.</remarks>
        /// <returns></returns>
        private static IQueryable<T> CreateQuery<T, TR>(
            this IQueryable<T> source, Expression<Func<IQueryable<T>, TR> > expression)
        {
            var queryExpression = ReplacingExpressionVisitor.Replace(
                expression.Parameters[0],
                source.Expression,
                expression.Body);

            return source.Provider.CreateQuery<T>(queryExpression);
        }

        private static Expression GetSourceExpression<TSource>(IEnumerable<TSource> source)
        {
            IQueryable<TSource> q = source as IQueryable<TSource>;
            if (q != null) return q.Expression;
            return Expression.Constant(source, typeof(IEnumerable<TSource>));
        }
    }
}