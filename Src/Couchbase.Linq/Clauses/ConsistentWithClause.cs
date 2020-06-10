using System;
using System.Linq.Expressions;
using Couchbase.Query;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Couchbase.Linq.Clauses
{
    /// <summary>
    /// Represents a clause on the query setting index consistency using <see cref="MutationState"/>.
    /// If applied multiple times to the same query, the states should be merged.
    /// </summary>
    internal class ConsistentWithClause : IBodyClause
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsistentWithClause" /> class.
        /// </summary>
        /// <param name="mutationState">Mutation state for the query.</param>
        public ConsistentWithClause(MutationState mutationState)
        {
            MutationState = mutationState;
        }

        /// <summary>
        /// Mutation state for the query.
        /// </summary>
        public MutationState MutationState { get; set; }

        /// <inheritdoc />
        public virtual void Accept(IQueryModelVisitor visitor, QueryModel queryModel, int index)
        {
            // Do nothing, the ClusterQueryExecutor will find this in QueryModel.BodyClauses to apply behavior
        }

        /// <inheritdoc />
        public IBodyClause Clone(CloneContext cloneContext)
        {
            return new ConsistentWithClause(MutationState);
        }

        /// <inheritdoc />
        public virtual void TransformExpressions(Func<Expression, Expression> transformation)
        {
            // Do nothing
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"with mutation state consistency {MutationState}";
        }
    }
}