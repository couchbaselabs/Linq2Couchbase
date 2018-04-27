using System;
using System.Text;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.QueryGeneration.FromParts
{
    /// <summary>
    /// Represents the FROM part of a query
    /// </summary>
    internal class FromPart : ExtentPart
    {
        public FromPart(IQuerySource querySource) : base(querySource)
        {
        }

        public override void AppendToStringBuilder(StringBuilder sb)
        {
            sb.Append(" FROM");

            base.AppendToStringBuilder(sb);
        }
    }
}
