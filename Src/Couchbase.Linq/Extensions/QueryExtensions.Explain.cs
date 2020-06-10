using System;
using System.Linq;
using System.Linq.Expressions;

namespace Couchbase.Linq.Extensions
{
    public static partial class QueryExtensions
    {
        /// <summary>
        /// Returns the query execution plan for the query.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>Explanation of the query</returns>
        public static dynamic Explain<T>(this IQueryable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var newExpression = Expression.Call(null,
                QueryExtensionMethods.Explain.MakeGenericMethod(typeof (T)),
                source.Expression);

            return source.Provider.Execute<dynamic>(newExpression);
        }
    }
}
