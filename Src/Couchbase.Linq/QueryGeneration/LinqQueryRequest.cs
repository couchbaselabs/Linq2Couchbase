using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Execution;
using Couchbase.N1QL;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Wraps a standard QueryRequest with some additional information used internally.
    /// </summary>
    internal class LinqQueryRequest : QueryRequest
    {
        private readonly ScalarResultBehavior _scalarResultBehavior;

        /// <summary>
        /// If true, indicates that the result of the query is wrapped in an object with a single property, named "result".
        /// After execution, this property should be extracted from the wrapper object.
        /// </summary>
        public ScalarResultBehavior ScalarResultBehavior
        {
            get { return _scalarResultBehavior; }
        }

        /// <summary>
        /// For queries returning a single result, true indicates that an empty result set should return the default value.
        /// For example, a call to .FirstOrDefault() would set this to true.
        /// </summary>
        public bool ReturnDefaultWhenEmpty { get; set; }

        /// <summary>
        /// Creates a new LinqQueryRequest, with the given N1QL query.
        /// </summary>
        /// <param name="query">N1QL query.</param>
        /// <param name="scalarResultBehavior">Behaviors related to extracting results for scalar queries.</param>
        public LinqQueryRequest(string query, ScalarResultBehavior scalarResultBehavior)
            : base(query)
        {
            if (scalarResultBehavior == null)
            {
                throw new ArgumentNullException("scalarResultBehavior");
            }

            _scalarResultBehavior = scalarResultBehavior;
        }

        #region Conversion Helpers

        private static readonly MethodInfo ToQueryRequestMethodInfo =
            typeof(LinqQueryRequest).GetMethod("ToQueryRequest", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Convert a LINQ query against a Couchbase bucket to a <see cref="LinqQueryRequest"/>.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="source">The query to be converted.  This must be a query based on a Couchbase bucket.</param>
        /// <returns><see cref="LinqQueryRequest"/> containing the N1QL query.</returns>
        internal static LinqQueryRequest CreateQueryRequest<T>(IQueryable<T> source)
        {
            return CreateQueryRequest<T, IQueryable<T>>(source, null);
        }

        /// <summary>
        /// Convert a LINQ query against a Couchbase bucket to a <see cref="LinqQueryRequest"/>.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <typeparam name="TResult">The type of the result returned by additionalExpresion.</typeparam>
        /// <param name="source">The query to be converted.  This must be a query based on a Couchbase bucket.</param>
        /// <param name="additionalExpression">Additional expressions to apply to the query before making a LinqQueryRequest.  Typically used for aggregates.</param>
        /// <returns><see cref="LinqQueryRequest"/> containing the N1QL query.</returns>
        internal static LinqQueryRequest CreateQueryRequest<T, TResult>(IQueryable<T> source,
            Expression<Func<IQueryable<T>, TResult>> additionalExpression)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!(source is IBucketQueryable))
            {
                throw new ArgumentException("CreateQueryRequest is only supported on Couchbase LINQ queries.", "source");
            }

            Expression sourceExpression = source.Expression;

            if (additionalExpression != null)
            {
                sourceExpression = ReplacingExpressionVisitor.Replace(
                    additionalExpression.Parameters[0],
                    sourceExpression,
                    additionalExpression.Body);
            }

            // Now wrap the completed query in a call to ToQueryRequest
            // This will trigger the creation of a ToQueryRequestResultOperator when building the QueryModel

            var newExpression = Expression.Call(null,
                ToQueryRequestMethodInfo.MakeGenericMethod(typeof(TResult)),
                sourceExpression);

            return source.Provider.Execute<LinqQueryRequest>(newExpression);
        }

        internal static LinqQueryRequest ToQueryRequest<TResult>(TResult source)
        {
            throw new NotImplementedException("ToQueryRequest should only be used to build expression trees, not executed.");
        }

        #endregion
    }
}
