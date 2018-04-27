using System;
using System.Text;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.QueryGeneration.FromParts
{
    /// <summary>
    /// Represents the FROM part of a query with an index JOIN clause.
    /// </summary>
    internal class IndexJoinPart : JoinPart
    {
        /// <summary>
        /// For joins, the expression for the key value to join
        /// </summary>
        public string OnKeys { get; set; }

        /// <summary>
        /// For index joins, the name of the extent on the left side of the join.  Should already be escaped.
        /// </summary>
        public string IndexJoinExtentName { get; set; }

        public IndexJoinPart(IQuerySource querySource) : base(querySource)
        {
        }

        public override void AppendToStringBuilder(StringBuilder sb)
        {
            base.AppendToStringBuilder(sb);

            sb.AppendFormat(" ON KEY {0} FOR {1}", OnKeys, IndexJoinExtentName);
        }
    }
}
