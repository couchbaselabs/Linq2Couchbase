using Couchbase.Core;
using Couchbase.N1QL;

namespace Couchbase.Linq
{
    /// <summary>
    /// A set of lower-level methods for other bucket-related functionality
    /// </summary>
    internal class BucketDataStore : IDataStore
    {
        private readonly IBucket _bucket;

        internal BucketDataStore(IBucket bucket)
        {
            _bucket = bucket;
        }

        /// <summary>
        /// Execute a N1QL function. Optionally specify parameters
        /// </summary>
        public IQueryResult<T> Execute<T>(string statement, params object[] parameters)
        {
            var query = new QueryRequest(statement)
                .AddPositionalParameter(parameters);
            return _bucket.Query<T>(query);
        }
    }
}