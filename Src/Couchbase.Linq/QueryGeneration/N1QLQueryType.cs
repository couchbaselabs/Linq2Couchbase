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
        /// Subquery against a bucket
        /// </summary>
        Subquery,

        /// <summary>
        /// Subquery against an array using the ARRAY keyword
        /// </summary>
        Array,

        /// <summary>
        /// Any operation performed on a nested array as a subquery
        /// </summary>
        ArrayAny,

        /// <summary>
        /// Any operation performed on a Couchbase bucket as the main query
        /// </summary>
        MainQueryAny,

        /// <summary>
        /// Any operation performed on a Couchbase bucket as a subquery
        /// </summary>
        SubqueryAny,

        /// <summary>
        /// All operation performed on a nested array as a subquery
        /// </summary>
        ArrayAll,

        /// <summary>
        /// All operation performed on a Couchbase bucket as the main query
        /// </summary>
        MainQueryAll,

        /// <summary>
        /// All operation performed on a Couchbase bucket as a subquery
        /// </summary>
        SubqueryAll,

        /// <summary>
        /// Represents a simple aggregate against a group.  Query returned will be the aggregate function call only.
        /// </summary>
        Aggregate
    }
}
