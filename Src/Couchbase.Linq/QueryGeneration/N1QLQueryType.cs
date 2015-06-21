namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Represents the type of query or subquery being generated
    /// </summary>
    public enum N1QlQueryType
    {
        /// <summary>
        /// Main SELECT statement
        /// </summary>
        Select = 0,

        /// <summary>
        /// Any operation performed on a nested array as a subquery
        /// </summary>
        Any,

        /// <summary>
        /// Any operation performed on the main query
        /// </summary>
        AnyMainQuery,

        /// <summary>
        /// All operation performed on a nested array as a subquery
        /// </summary>
        All,

        /// <summary>
        /// All operation performed on the main query
        /// </summary>
        AllMainQuery
    }
}
