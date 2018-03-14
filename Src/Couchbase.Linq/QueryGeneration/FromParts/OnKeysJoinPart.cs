using System.Text;

namespace Couchbase.Linq.QueryGeneration.FromParts
{
    /// <summary>
    /// Represents the FROM part of a query with an ON KEYS based JOIN clause.
    /// </summary>
    internal class OnKeysJoinPart : JoinPart
    {
        /// <summary>
        /// For joins, the expression for the key value to join
        /// </summary>
        public string OnKeys { get; set; }

        public override void AppendToStringBuilder(StringBuilder sb)
        {
            base.AppendToStringBuilder(sb);

            sb.AppendFormat(" ON KEYS {0}", OnKeys);
        }
    }
}
