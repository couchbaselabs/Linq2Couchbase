using System.Text;

namespace Couchbase.Linq.QueryGeneration.FromParts
{
    /// <summary>
    /// Represents the FROM part of a query with a JOIN, NEST, or UNNEST clause
    /// </summary>
    internal class JoinPart : ExtentPart
    {
        /// <summary>
        /// Type of join to perform
        /// </summary>
        public string JoinType { get; set; }

        public override void AppendToStringBuilder(StringBuilder sb)
        {
            sb.Append(' ');
            sb.Append(JoinType);

            base.AppendToStringBuilder(sb);
        }
    }
}
