using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Couchbase.IO.Operations;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Extensions
{
    public class ExplainResultOperator : SequenceTypePreservingResultOperatorBase
    {
        public override StreamedSequence ExecuteInMemory<T>(StreamedSequence input)
        {
            return input; //no change to sequence
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new ExplainResultOperator();
        }

        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
            //no parameters so just ignore this
            //throw new NotImplementedException();
        }
    }
}
