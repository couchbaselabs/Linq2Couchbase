
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Represents the FROM part of a query
    /// </summary>
    internal class N1QlFromQueryPart
    {

        /// <summary>
        /// Source of the query data, such as a bucket name.  Should already be escaped.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Name of the query data when being referenced in the query ("as" clause).  Should already be escaped.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Type of join to perform
        /// </summary>
        public string JoinType { get; set; }

        /// <summary>
        /// For joins, the expression for the key value to join
        /// </summary>
        public string OnKeys { get; set; }

    }
}
