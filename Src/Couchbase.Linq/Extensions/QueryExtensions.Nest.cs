using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Linq.Metadata;

namespace Couchbase.Linq.Extensions
{
    public static partial class QueryExtensions
    {
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
                throw new ArgumentNullException(nameof(inner));
            }
            if (outer == null)
            {
                throw new ArgumentNullException(nameof(outer));
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
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
                        EnumerableExtensionMethods.Nest.MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TResult)),
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
                        QueryExtensionMethods.Nest.MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TResult)),
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
                throw new ArgumentNullException(nameof(inner));
            }
            if (outer == null)
            {
                throw new ArgumentNullException(nameof(outer));
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
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
                        EnumerableExtensionMethods.LeftOuterNest.MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TResult)),
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
                        QueryExtensionMethods.LeftOuterNest.MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TResult)),
                        outer.Expression,
                        GetSourceExpression(inner),
                        Expression.Quote(keySelector),
                        Expression.Quote(resultSelector)));
            }
        }
    }
}
