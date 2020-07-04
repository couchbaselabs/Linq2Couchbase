using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Linq.Extensions
{
    public static partial class QueryExtensions
    {
        /// <summary>
        /// Returns the query execution plan for the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/>.</param>
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

        /// <summary>
        /// Returns the query execution plan for the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/>.</param>
        /// <returns>Explanation of the query</returns>
        public static Task<dynamic> ExplainAsync<T>(this IQueryable<T> source) =>
            ExplainAsync(source, default);

        /// <summary>
        /// Returns the query execution plan for the query.
        /// </summary>
        /// <typeparam name="T">Type of item to query.</typeparam>
        /// <param name="source">Source <see cref="IQueryable{T}"/>.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Explanation of the query</returns>
        public static Task<dynamic> ExplainAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<T, Task<dynamic>>(QueryExtensionMethods.ExplainAsync, source, null, cancellationToken);
        }
    }
}
