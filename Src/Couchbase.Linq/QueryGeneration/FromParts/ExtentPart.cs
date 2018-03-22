using System.Text;

namespace Couchbase.Linq.QueryGeneration.FromParts
{
    /// <summary>
    /// Represents an extent of the query.
    /// </summary>
    internal abstract class ExtentPart
    {
        /// <summary>
        /// Source of the query data, such as a bucket name.  Should already be escaped.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Name of the query data when being referenced in the query ("as" clause).  Should already be escaped.
        /// </summary>
        public string ItemName { get; set; }

        public virtual void AppendToStringBuilder(StringBuilder sb)
        {
            sb.AppendFormat(" {0} as {1}",
                Source,
                ItemName);
        }
    }
}
