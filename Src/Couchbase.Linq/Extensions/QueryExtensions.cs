using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Couchbase.Linq.Extensions
{
    public static class QueryExtensions
    {
        /// <summary>
        /// Where Missing Clause for N1QL. (.WhereMissing(e -> e.Age) translates to WHERE table/alias.Age IS MISSING)
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
                    ((MethodInfo)MethodBase.GetCurrentMethod())
                        .MakeGenericMethod(typeof(T), typeof(T1)),
                    source.Expression,
                    Expression.Quote(predicate)));

        }
    }
}

