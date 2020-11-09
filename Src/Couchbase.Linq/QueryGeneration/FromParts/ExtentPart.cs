using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase.Linq.Clauses;
using Remotion.Linq.Clauses;

#nullable disable

namespace Couchbase.Linq.QueryGeneration.FromParts
{
    /// <summary>
    /// Represents an extent of the query.
    /// </summary>
    internal abstract class ExtentPart
    {
        private IQuerySource _querySource;

        /// <summary>
        /// Clause which is the source of the query data, such as a MainFromClause.
        /// </summary>
        public IQuerySource QuerySource
        {
            get => _querySource;
            set => _querySource = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Source of the query data, such as a bucket name.  Should already be escaped.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Name of the query data when being referenced in the query ("as" clause).  Should already be escaped.
        /// </summary>
        public string ItemName { get; set; }

        public List<HintClause> Hints { get; set; }

        protected ExtentPart(IQuerySource querySource)
        {
            QuerySource = querySource ?? throw new ArgumentNullException(nameof(querySource));
        }

        public virtual void AppendToStringBuilder(StringBuilder sb)
        {
            // Use a single format string for performance

            if (Hints != null && Hints.Any())
            {
                sb.AppendFormat(" {0} as {1} USE ",
                    Source,
                    ItemName);

                for (var i = 0; i < Hints.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(' ');
                    }

                    Hints[i].AppendToStringBuilder(sb);
                }
            }
            else
            {
                sb.AppendFormat(" {0} as {1}",
                    Source,
                    ItemName);
            }
        }
    }
}
