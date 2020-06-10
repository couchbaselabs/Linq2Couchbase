using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Linq.Metadata;

namespace Couchbase.Linq.Extensions
{
    // UseXXX extensions

    public static partial class QueryExtensions
    {
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
                throw new ArgumentNullException(nameof(items));
            }
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
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
                        EnumerableExtensionMethods.UseKeys.MakeGenericMethod(typeof(T)),
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
                        QueryExtensionMethods.UseKeys.MakeGenericMethod(typeof(T)),
                        items.Expression,
                        GetSourceExpression(keys)));
            }
        }

        #endregion

        #region UseIndex

        /// <summary>
        /// Provides an index hint to the query engine.
        /// </summary>
        /// <typeparam name="T">Type of items being queried.</typeparam>
        /// <param name="source">Items being queried.</param>
        /// <param name="indexName">Name of the index to use.</param>
        /// <returns>Modified IQueryable</returns>
        public static IQueryable<T> UseIndex<T>(this IQueryable<T> source, string indexName) =>
            source.UseIndex(indexName, N1QlIndexType.Gsi);

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
                throw new ArgumentNullException(nameof(source));
            }
            if (indexType < N1QlIndexType.Gsi || indexType > N1QlIndexType.View)
            {
                throw new ArgumentOutOfRangeException(nameof(indexType));
            }

            return source.Provider.CreateQuery<T>(
                Expression.Call(
                    QueryExtensionMethods.UseIndexWithType.MakeGenericMethod(typeof(T)),
                    source.Expression,
                    Expression.Constant(indexName),
                    Expression.Constant(indexType)));
        }

        #endregion

        #region Use Hash

        /// <summary>
        /// Provides an hash join hint to the query engine.
        /// </summary>
        /// <typeparam name="T">Type of items being queried.</typeparam>
        /// <param name="source">Items being queried.</param>
        /// <param name="type">Type of hash hint to provide.</param>
        /// <returns>Modified IQueryable</returns>
        /// <remarks>Only valid when using Couchbase Server 5.5 Enterprise Edition (or later).  Not supported by Community Edition.</remarks>
        public static IQueryable<T> UseHash<T>(this IQueryable<T> source, HashHintType type)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (type < HashHintType.Build || type > HashHintType.Probe)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            return source.Provider.CreateQuery<T>(
                Expression.Call(
                    QueryExtensionMethods.UseHash.MakeGenericMethod(typeof(T)),
                    source.Expression,
                    Expression.Constant(type)));
        }

        #endregion
    }
}
