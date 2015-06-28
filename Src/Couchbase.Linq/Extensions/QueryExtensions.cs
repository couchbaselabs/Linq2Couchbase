using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Couchbase.Linq.Metadata;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;

namespace Couchbase.Linq.Extensions
{
    public static class QueryExtensions
    {
        /// <summary>
        ///     Where Missing Clause for N1QL. (.WhereMissing(e -> e.Age) translates to WHERE table/alias.Age IS MISSING)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereMissing<T, T1>(this IQueryable<T> source, Expression<Func<T, T1>> predicate)
        {
            return source.Provider.CreateQuery<T>(
                Expression.Call(
                    ((MethodInfo) MethodBase.GetCurrentMethod())
                        .MakeGenericMethod(typeof (T), typeof (T1)),
                    source.Expression,
                    Expression.Quote(predicate)));
        }

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
            var queryExpression = ReplacingExpressionTreeVisitor.Replace(
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