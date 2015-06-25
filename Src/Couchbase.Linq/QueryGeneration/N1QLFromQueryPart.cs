
namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Represents the FROM part of a query
    /// </summary>
    public class N1QlFromQueryPart
    {

        /// <summary>
        /// Source of the query data, such as a bucket name.  Should already be escaped.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Name of the query data when being referenced in the query ("as" clause).  Should already be escaped.
        /// </summary>
        public string ItemName { get; set; }

    }
}
