using System;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Operators
{
    /// <summary>
    /// A ResultOperator for the "META" function for query parsing.
    /// </summary>
    public class MetaResultOperator : SequenceTypePreservingResultOperatorBase
    {
        public override StreamedSequence ExecuteInMemory<T>(StreamedSequence input)
        {
            return input; //no change to sequence
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new MetaResultOperator();
        }

        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
            //no parameters so just ignore this
        }
    }
}
