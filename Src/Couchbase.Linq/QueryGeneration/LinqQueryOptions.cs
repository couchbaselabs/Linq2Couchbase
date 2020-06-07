using System;
using Couchbase.Linq.Execution;
using Couchbase.Query;

namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Wraps a standard QueryOptions with some additional information used internally.
    /// </summary>
    internal class LinqQueryOptions : QueryOptions
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
        /// <param name="scalarResultBehavior">Behaviors related to extracting results for scalar queries.</param>
        public LinqQueryOptions(ScalarResultBehavior scalarResultBehavior)
        {
            _scalarResultBehavior = scalarResultBehavior ?? throw new ArgumentNullException("scalarResultBehavior");
        }
    }
}
