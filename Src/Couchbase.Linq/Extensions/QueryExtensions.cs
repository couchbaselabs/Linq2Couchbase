using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Couchbase.Linq.Execution;

namespace Couchbase.Linq.Extensions
{
    /// <summary>
    /// Extensions to <see cref="IQueryable{T}" /> for use in queries against a <see cref="CollectionContext"/>.
    /// </summary>
    public static partial class QueryExtensions
    {
        private static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            Expression expression,
            CancellationToken cancellationToken = default)
        {
            if (source.Provider is IAsyncQueryProvider provider)
            {
                if (operatorMethodInfo.IsGenericMethod)
                {
                    operatorMethodInfo
                        = operatorMethodInfo.GetGenericArguments().Length == 2
                            ? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult).GetGenericArguments().Single())
                            : operatorMethodInfo.MakeGenericMethod(typeof(TSource));
                }

                var updatedExpression = Expression.Call(
                    instance: null,
                    method: operatorMethodInfo,
                    arguments: expression == null
                        ? new[] {source.Expression}
                        : new[] {source.Expression, expression});

                return provider.ExecuteAsync<TResult>(updatedExpression, cancellationToken);
            }

            throw new InvalidOperationException("The provided IQueryable is not backed by an IAsyncQueryProvider.");
        }

        private static Expression GetSourceExpression<TSource>(IEnumerable<TSource> source)
        {
            if (source is IQueryable<TSource> q)
            {
                return q.Expression;
            }

            return Expression.Constant(source, typeof(IEnumerable<TSource>));
        }
    }
}