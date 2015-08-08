using System;
using System.Linq.Expressions;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.Clauses
{
    public class UseKeysClause : IBodyClause
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UseKeysClause" /> class.
        /// </summary>
        /// <param name="keys">Expression used to get the keys from each item in the outer sequence</param>
        public UseKeysClause(Expression keys)
        {
            Keys = keys;
        }

        /// <summary>
        ///     Gets the expression used to get the keys being selected
        /// </summary>
        public Expression Keys { get; set; }

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
            if (visotorx != null) visotorx.VisitUseKeysClause(this, queryModel, index);
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
            Keys = transformation(Keys);
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
        public virtual UseKeysClause Clone(CloneContext cloneContext)
        {
            var clone = new UseKeysClause(Keys);
            return clone;
        }

        public override string ToString()
        {
            return String.Format("use keys {0}", Keys);
        }
    }
}