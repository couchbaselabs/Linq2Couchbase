using System.Text;

namespace Couchbase.Linq.QueryGeneration.FromParts
{
    /// <summary>
    /// Represents the FROM part of a query
    /// </summary>
    internal class FromPart : ExtentPart
    {
        public override void AppendToStringBuilder(StringBuilder sb)
        {
            sb.Append(" FROM");

            base.AppendToStringBuilder(sb);
        }
    }
}
