using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Couchbase.Core.Buckets;
using Couchbase.Linq.Execution;
using Couchbase.Linq.Metadata;
using Couchbase.Linq.QueryGeneration;
using Couchbase.N1QL;
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
                        typeof(QueryExtensions).GetMethod("Nest")
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
                        typeof(QueryExtensions).GetMethod("LeftOuterNest")
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
                        typeof(QueryExtensions).GetMethod("UseKeys")
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
                typeof(QueryExtensions).GetMethod("Explain")
                    .MakeGenericMethod(typeof (T)),
                source.Expression);

            return source.Provider.Execute<dynamic>(newExpression);
        }

        #region Use Index

        private static readonly MethodInfo UseIndexMethod =
            typeof(QueryExtensions).GetMethods()
            .First(p => p.Name == "UseIndex" && p.GetParameters().Length == 3);

        /// <summary>
        /// Provides an index hint to the query engine.
        /// </summary>
        /// <typeparam name="T">Type of items being queried.</typeparam>
        /// <param name="source">Items being queried.</param>
        /// <param name="indexName">Name of the index to use.</param>
        /// <returns>Modified IQueryable</returns>
        public static IQueryable<T> UseIndex<T>(this IQueryable<T> source, string indexName)
        {
            return source.UseIndex(indexName, N1QlIndexType.Gsi);
        }

        /// <summary>
        /// Provides an index hint to the query engine.
        /// </summary>
        /// <typeparam name="T">Type of items being queried.</typeparam>
        /// <param name="source">Items being queried.</param>
        /// <param name="indexName">Name of the index to use.</param>
        /// <param name="indexType">Type of the index to use.</param>
        /// <returns>Modified IQueryable</returns>
        public static IQueryable<T> UseIndex<T>(this IQueryable<T> source, string indexName, N1QlIndexType indexType)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!Enum.IsDefined(typeof(N1QlIndexType), indexType))
            {
                throw new ArgumentOutOfRangeException("indexType");
            }

            return source.Provider.CreateQuery<T>(
                    Expression.Call(
                        UseIndexMethod
                            .MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Constant(indexName),
                        Expression.Constant(indexType)));
        }

        #endregion

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
            EnsureBucketQueryable(source, "ExecuteAsync", "source");

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
            EnsureBucketQueryable(source, "ExecuteAsync", "source");

            if (typeof (TResult).GetTypeInfo().IsGenericTypeDefinition &&
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

        #region Query Request Settings

        /// <summary>
        /// Specifies the consistency guarantee/constraint for index scanning.
        /// </summary>
        /// <param name="source">Sets scan consistency for this query.  Must be a Couchbase LINQ query.</param>
        /// <param name="scanConsistency">Specify the consistency guarantee/constraint for index scanning.</param>
        /// <remarks>The default is <see cref="ScanConsistency.NotBounded"/>.</remarks>
        public static IQueryable<T> ScanConsistency<T>(this IQueryable<T> source, ScanConsistency scanConsistency)
        {
            EnsureBucketQueryable(source, "ScanConsistency", "source");

            ((IBucketQueryExecutorProvider) source).BucketQueryExecutor.ScanConsistency = scanConsistency;

            return source;
        }

        /// <summary>
        /// Specifies the maximum time the client is willing to wait for an index to catch up to the consistency requirement in the request.
        /// If an index has to catch up, and the time is exceed doing so, an error is returned.
        /// </summary>
        /// <param name="source">Sets scan wait for this query.  Must be a Couchbase LINQ query.</param>
        /// <param name="scanWait">The maximum time the client is willing to wait for index to catch up to the vector timestamp.</param>
        public static IQueryable<T> ScanWait<T>(this IQueryable<T> source, TimeSpan scanWait)
        {
            EnsureBucketQueryable(source, "ScanWait", "source");

            ((IBucketQueryExecutorProvider)source).BucketQueryExecutor.ScanWait = scanWait;

            return source;
        }

        /// <summary>
        /// Requires that the indexes but up to date with a <see cref="MutationState"/> before the query is executed.
        /// </summary>
        /// <param name="source">Sets consistency requirement for this query.  Must be a Couchbase LINQ query.</param>
        /// <param name="state"><see cref="MutationState"/> used for conistency controls.</param>
        /// <remarks>If called multiple times, the states from the calls are combined.</remarks>
        public static IQueryable<T> ConsistentWith<T>(this IQueryable<T> source, MutationState state)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!(source is IBucketQueryExecutorProvider))
            {
                // do nothing if this isn't a Couchbase LINQ query
                return source;
            }

            ((IBucketQueryExecutorProvider) source).BucketQueryExecutor.ConsistentWith(state);

            return source;
        }

        /// <summary>
        /// Specifies if the query results should be streamed, reducing memory utilzation for large result sets.
        /// </summary>
        /// <param name="source">Sets scan consistency for this query.  Must be a Couchbase LINQ query.</param>
        /// <param name="useStreaming">Specifies if query results should be streamed.</param>
        /// <remarks>
        /// Streaming is not supported in combination with change tracking.  If change tracking is enabled,
        /// this setting will be ignored.  Streaming is enabled by default.
        /// </remarks>
        public static IQueryable<T> UseStreaming<T>(this IQueryable<T> source, bool useStreaming)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!(source is IBucketQueryExecutorProvider))
            {
                // do nothing if this isn't a Couchbase LINQ query
                return source;
            }

            ((IBucketQueryExecutorProvider)source).BucketQueryExecutor.UseStreaming = useStreaming;

            return source;
        }

        #endregion

        private static void EnsureBucketQueryable<T>(IQueryable<T> source, string methodName, string paramName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (!(source is IBucketQueryable) || !(source is IBucketQueryExecutorProvider))
            {
                throw new ArgumentException(string.Format("{0} is only supported on Couchbase LINQ queries.", methodName), paramName);
            }
        }

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