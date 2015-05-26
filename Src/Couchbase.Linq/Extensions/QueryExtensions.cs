using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        /// <summary>
        /// The EXPLAIN statement is used before any N1QL statement to obtain information about how the statement operates.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IQueryable<T> Explain<T>(this IQueryable<T> source)
        {
            return CreateQuery(source, queryable => queryable.Explain());
        }

        /// <summary>
        /// An expression generation helper for adding additional methods to a Linq provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TR">The type of the return value.</typeparam>
        /// <param name="source">The <see cref="IQueryable"/> source.</param>
        /// <param name="expression">The expression.</param>
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
    }
}