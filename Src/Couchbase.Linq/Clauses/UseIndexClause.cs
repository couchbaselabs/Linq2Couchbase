using System;
using System.Text;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.Clauses
{
    internal class UseIndexClause : HintClause
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UseIndexClause" /> class.
        /// </summary>
        /// <param name="indexName">Name of the index to use.</param>
        /// <param name="indexType">Type of the index to use.</param>
        public UseIndexClause(string indexName, N1QlIndexType indexType)
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            IndexName = indexName;
            IndexType = indexType;
        }

        /// <summary>
        ///     Name of the index to use.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        ///     Type of the index to use.
        /// </summary>
        public N1QlIndexType IndexType { get; set; }

        /// <summary>
        ///     Clones this clause.
        /// </summary>
        /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext" />.</param>
        /// <returns></returns>
        public override HintClause Clone(CloneContext cloneContext)
        {
            return new UseIndexClause(IndexName, IndexType);
        }

        public override void AppendToStringBuilder(StringBuilder sb)
        {
            var indexTypeStr = IndexType == N1QlIndexType.Gsi ? "GSI" : "VIEW";
            sb.AppendFormat("INDEX ({0} USING {1})", N1QlHelpers.EscapeIdentifier(IndexName), indexTypeStr);
        }

        public override string ToString()
        {
            return $"use index {IndexName} using {IndexType.ToString().ToUpper()}";
        }
    }
}