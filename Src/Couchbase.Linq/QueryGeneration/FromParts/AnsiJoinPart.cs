using System;
using System.Text;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.QueryGeneration.FromParts
{
    /// <summary>
    /// Represents the FROM part of a query with a full ANSI JOIN clause.
    /// </summary>
    internal class AnsiJoinPart : JoinPart
    {
        /// <summary>
        /// Outer key selector.
        /// </summary>
        public string OuterKey { get; set; }

        /// <summary>
        /// Inner key selector.
        /// </summary>
        public string InnerKey { get; set; }

        /// <summary>
        /// Join operator, defaults to "="
        /// </summary>
        public string Operator { get; set; } = "=";

        /// <summary>
        /// Additional predicates to apply to the inner sequence.
        /// </summary>
        public string AdditionalInnerPredicates { get; set; }

        public AnsiJoinPart(IQuerySource querySource) : base(querySource)
        {
        }

        public override void AppendToStringBuilder(StringBuilder sb)
        {
            base.AppendToStringBuilder(sb);

            sb.AppendFormat(" ON ({0} {1} {2})", OuterKey, Operator, InnerKey);

            if (!string.IsNullOrEmpty(AdditionalInnerPredicates))
            {
                sb.AppendFormat(" AND {0}", AdditionalInnerPredicates);
            }
        }
    }
}
