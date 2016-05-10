
using System.Text;
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

        /// <summary>
        /// For index joins, the name of the extent on the left side of the join.  Should already be escaped.
        /// </summary>
        public string IndexJoinExtentName { get; set; }

        /// <summary>
        /// If true, indicates that this is an index join where the primary key reference is on the left side instead of the right side.
        /// </summary>
        public bool IsIndexJoin
        {
            get { return !string.IsNullOrEmpty(IndexJoinExtentName); }
        }

        public void AppendToStringBuilder(StringBuilder sb)
        {
            sb.AppendFormat(" {0} {1} as {2}",
                JoinType,
                Source,
                ItemName);

            if (!string.IsNullOrEmpty(OnKeys))
            {
                if (!IsIndexJoin)
                {
                    sb.AppendFormat(" ON KEYS {0}", OnKeys);
                }
                else
                {
                    sb.AppendFormat(" ON KEY {0} FOR {1}", OnKeys, IndexJoinExtentName);
                }
            }
        }
    }
}
