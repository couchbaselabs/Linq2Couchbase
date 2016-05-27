using Couchbase.N1QL;

namespace Couchbase.Linq
{
    /// <summary>
    /// This interface defines a set of lower-level methods for other bucket-related functionality
    /// </summary>
    public interface IDataStore
    {
        /// <summary>
        /// Execute a N1QL function. Optionally specify parameters
        /// </summary>
        IQueryResult<T> Execute<T>(string statement, params object[] parameters);
    }
}