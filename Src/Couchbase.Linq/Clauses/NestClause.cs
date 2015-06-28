using System;
using System.Linq.Expressions;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.Clauses
{
    public class NestClause : IBodyClause, IQuerySource
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NestClause" /> class.
        /// </summary>
        /// <param name="itemName">Name of the item returned by the nest clause</param>
        /// <param name="itemType">Type of the item returned by the nest clause</param>
        /// <param name="inner">The inner sequence being nested</param>
        /// <param name="keySelector">Expression used to get the keys from each item in the outer sequence</param>
        public NestClause(string itemName, Type itemType, Expression inner, Expression keySelector, bool isLeftOuterNest)
        {
            ItemName = itemName;
            ItemType = itemType;
            InnerSequence = inner;
            KeySelector = keySelector;
            IsLeftOuterNest = isLeftOuterNest;
        }

        public string ItemName { get; set; }

        public Type ItemType { get; set; }

        /// <summary>
        ///     Gets the inner sequence being nested
        /// </summary>
        public Expression InnerSequence { get; set; }

        /// <summary>
        ///     Gets the expression used to get the keys from each item in the outer sequence
        /// </summary>
        public Expression KeySelector { get; set; }

        public bool IsLeftOuterNest { get; set; }

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
            if (visotorx != null) visotorx.VisitNestClause(this, queryModel, index);
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
            InnerSequence = transformation(InnerSequence);
            KeySelector = transformation(KeySelector);
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
        public virtual NestClause Clone(CloneContext cloneContext)
        {
            var clone = new NestClause(ItemName, ItemType, InnerSequence, KeySelector, IsLeftOuterNest);
            return clone;
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} in {3} on keys {4}",
                IsLeftOuterNest ? "left outer nest" : "nest",
                ItemType.Name,
                ItemName,
                InnerSequence,
                KeySelector);
        }
    }
}