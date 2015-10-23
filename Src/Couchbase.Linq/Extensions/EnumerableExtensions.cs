using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Metadata;

namespace Couchbase.Linq.Extensions
{
    public static class EnumerableExtensions
    {

        #region Nest

        /// <summary>
        ///     Emulates Nest for N1QL against an IEnumerable
        /// </summary>
        /// <typeparam name="TOuter">Type of the source sequence</typeparam>
        /// <typeparam name="TInner">Type of the inner sequence being nested</typeparam>
        /// <typeparam name="TResult">Type of the result sequence</typeparam>
        /// <param name="outer"></param>
        /// <param name="inner">Sequence to be nested</param>
        /// <param name="keySelector">Expression to get the list of keys to nest for an item in the source sequence.  Should return a list of strings.</param>
        /// <param name="resultSelector">Expression that returns the result</param>
        /// <returns></returns>
        public static IEnumerable<TResult> Nest<TOuter, TInner, TResult>(
            this IEnumerable<TOuter> outer, IEnumerable<TInner> inner,
            Func<TOuter, IEnumerable<string>> keySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector) where TInner : IDocumentMetadataProvider
        {
            return outer.Nest(inner, keySelector, resultSelector, true);
        }

        /// <summary>
        ///     Emulates LeftNest for N1QL against an IEnumerable
        /// </summary>
        /// <typeparam name="TOuter">Type of the source sequence</typeparam>
        /// <typeparam name="TInner">Type of the inner sequence being nested</typeparam>
        /// <typeparam name="TResult">Type of the result sequence</typeparam>
        /// <param name="outer"></param>
        /// <param name="inner">Sequence to be nested</param>
        /// <param name="keySelector">Expression to get the list of keys to nest for an item in the source sequence.  Should return a list of strings.</param>
        /// <param name="resultSelector">Expression that returns the result</param>
        /// <returns></returns>
        public static IEnumerable<TResult> LeftOuterNest<TOuter, TInner, TResult>(
            this IEnumerable<TOuter> outer, IEnumerable<TInner> inner,
            Func<TOuter, IEnumerable<string>> keySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector) where TInner : IDocumentMetadataProvider
        {
            return outer.Nest(inner, keySelector, resultSelector, false);
        }

        /// <summary>
        ///     Emulates Nest for N1QL against an IEnumerable
        /// </summary>
        /// <typeparam name="TOuter">Type of the source sequence</typeparam>
        /// <typeparam name="TInner">Type of the inner sequence being nested</typeparam>
        /// <typeparam name="TResult">Type of the result sequence</typeparam>
        /// <param name="outer"></param>
        /// <param name="inner">Sequence to be nested</param>
        /// <param name="keySelector">Expression to get the list of keys to nest for an item in the source sequence.  Should return a list of strings.</param>
        /// <param name="resultSelector">Expression that returns the result</param>
        /// <param name="innerNest">Excludes results where no matches are found in the inner sequence</param>
        /// <returns></returns>
        private static IEnumerable<TResult> Nest<TOuter, TInner, TResult>(
            this IEnumerable<TOuter> outer, IEnumerable<TInner> inner,
            Func<TOuter, IEnumerable<string>> keySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            bool innerNest) where TInner : IDocumentMetadataProvider
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

            // Create a dictionary on the inner sequence, based on document key
            // This ensures that the inner sequence is only enumerated once
            // And that lookups are fast
            var innerDictionary = inner.ToDictionary(p => N1QlFunctions.Key(p));

            return outer
                .Select(outerDocument =>
                {
                    var keys = keySelector.Invoke(outerDocument);

                    List<TInner> innerDocuments = null;
                    if (keys != null)
                    {
                        innerDocuments = keys.Select(key =>
                        {
                            TInner innerDocument;
                            if (!innerDictionary.TryGetValue(key, out innerDocument))
                            {
                                innerDocument = default(TInner); // return null when not found
                            }

                            return innerDocument;
                        })
                            .Where(p => p != null) // skip any documents that weren't found in the dictionary
                            .ToList();
                    }

                    if (innerNest && (innerDocuments == null || innerDocuments.Count == 0))
                    {
                        // For inner nest, return null if there are no inner documents
                        return default(TResult);
                    }

                    return resultSelector.Invoke(outerDocument, innerDocuments);
                })
                .Where(p => p != null); // Filter out any null results returned due to inner nest or a null from the resultSelector
        }

        #endregion

        #region UseKeys

        /// <summary>
        ///     Emulates UseKeys for N1QL against an IEnumerable
        /// </summary>
        /// <typeparam name="T">Type of the source sequence</typeparam>
        /// <param name="items">Items being filtered</param>
        /// <param name="keys">Keys to be selected</param>
        /// <returns></returns>
        public static IEnumerable<T> UseKeys<T>(
            this IEnumerable<T> items, IEnumerable<string> keys) where T : IDocumentMetadataProvider
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            return items.Where(p => keys.Contains(N1QlFunctions.Key(p)));
        }

        #endregion

    }
}
