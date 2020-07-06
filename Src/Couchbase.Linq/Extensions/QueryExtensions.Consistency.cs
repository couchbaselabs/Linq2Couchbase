using System;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Query;

namespace Couchbase.Linq.Extensions
{
    public static partial class QueryExtensions
    {
        /// <summary>
        /// Specifies the consistency guarantee/constraint for index scanning.
        /// </summary>
        /// <param name="source">Sets scan consistency for this query.  Must be a Couchbase LINQ query.</param>
        /// <param name="scanConsistency">Specify the consistency guarantee/constraint for index scanning.</param>
        /// <remarks>The default is <see cref="QueryScanConsistency.NotBounded"/>.</remarks>
        public static IQueryable<T> ScanConsistency<T>(this IQueryable<T> source, QueryScanConsistency scanConsistency)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider.CreateQuery<T>(
                Expression.Call(
                    QueryExtensionMethods.ScanConsistency.MakeGenericMethod(typeof(T)),
                    source.Expression,
                    Expression.Constant(scanConsistency)));
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
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider.CreateQuery<T>(
                Expression.Call(
                    QueryExtensionMethods.ConsistentWith.MakeGenericMethod(typeof(T)),
                    source.Expression,
                    Expression.Constant(state)));
        }

        /// <summary>
        /// Requires that the indexes but up to date with a <see cref="MutationState"/> before the query is executed.
        /// </summary>
        /// <param name="source">Sets consistency requirement for this query.  Must be a Couchbase LINQ query.</param>
        /// <param name="state"><see cref="MutationState"/> used for consistency controls.</param>
        /// <param name="scanWait">Time to wait for index scan.</param>
        /// <remarks>If called multiple times, the states from the calls are combined.</remarks>
        public static IQueryable<T> ConsistentWith<T>(this IQueryable<T> source, MutationState state, TimeSpan scanWait)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider.CreateQuery<T>(
                Expression.Call(
                    QueryExtensionMethods.ConsistentWithScanWait.MakeGenericMethod(typeof(T)),
                    source.Expression,
                    Expression.Constant(state),
                    Expression.Constant(scanWait)));
        }
    }
}
