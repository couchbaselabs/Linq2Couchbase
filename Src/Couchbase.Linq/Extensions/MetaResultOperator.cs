using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Extensions
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
