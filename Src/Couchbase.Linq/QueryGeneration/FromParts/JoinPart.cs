using System.Text;
using Remotion.Linq.Clauses;

#nullable disable

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

        public JoinPart(IQuerySource querySource) : base(querySource)
        {
        }

        public override void AppendToStringBuilder(StringBuilder sb)
        {
            sb.Append(' ');
            sb.Append(JoinType);

            base.AppendToStringBuilder(sb);
        }
    }
}
