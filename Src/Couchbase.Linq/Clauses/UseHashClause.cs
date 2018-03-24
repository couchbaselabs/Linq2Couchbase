using System;
using System.Text;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.Clauses
{
    internal class UseHashClause : HintClause
    {
        public UseHashClause(HashHintType hashType)
        {
            HashHintType = hashType;
        }

        public HashHintType HashHintType { get; set; }

        /// <summary>
        ///     Clones this clause.
        /// </summary>
        /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext" />.</param>
        /// <returns></returns>
        public override HintClause Clone(CloneContext cloneContext)
        {
            return new UseHashClause(HashHintType);
        }

        public override void AppendToStringBuilder(StringBuilder sb)
        {
            var hashTypeStr = HashHintType == HashHintType.Build ? "build" : "probe";
            sb.AppendFormat("HASH ({0})", hashTypeStr);
        }

        public override string ToString()
        {
            return $"use hash ({HashHintType})";
        }
    }
}