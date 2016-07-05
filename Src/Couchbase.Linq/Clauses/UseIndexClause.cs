using System;
using System.Linq.Expressions;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.Clauses
{
    internal class UseIndexClause : IBodyClause
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
                throw new ArgumentNullException("indexName");
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
        ///     Accepts the specified visitor
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        /// <param name="queryModel">The query model in whose context this clause is visited.</param>
        /// <param name="index">
        ///     The index of this clause in the <paramref name="queryModel" />'s
        ///     <see cref="QueryModel.BodyClauses" /> collection.
        /// </param>
        public virtual void Accept(IQueryModelVisitor visitor, QueryModel queryModel, int index)
        {
            var visotorx = visitor as IN1QlQueryModelVisitor;
            if (visotorx != null) visotorx.VisitUseIndexClause(this, queryModel, index);
        }

        /// <summary>
        ///     Transforms all the expressions in this clause and its child objects via the given
        ///     <paramref name="transformation" /> delegate.
        /// </summary>
        /// <param name="transformation">
        ///     The transformation object. This delegate is called for each <see cref="Expression" /> within this
        ///     clause, and those expressions will be replaced with what the delegate returns.
        /// </param>
        public void TransformExpressions(Func<Expression, Expression> transformation)
        {
        }

        IBodyClause IBodyClause.Clone(CloneContext cloneContext)
        {
            return Clone(cloneContext);
        }

        /// <summary>
        ///     Clones this clause.
        /// </summary>
        /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext" />.</param>
        /// <returns></returns>
        public virtual UseIndexClause Clone(CloneContext cloneContext)
        {
            var clone = new UseIndexClause(IndexName, IndexType);
            return clone;
        }

        public override string ToString()
        {
            return string.Format("use index {0} using {1}", IndexName, IndexType.ToString().ToUpper());
        }
    }
}