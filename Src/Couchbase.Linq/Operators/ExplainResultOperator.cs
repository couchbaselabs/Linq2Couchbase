using System;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Operators
{
    internal class ExplainResultOperator : ValueFromSequenceResultOperatorBase
    {
        public override StreamedValue ExecuteInMemory<T>(StreamedSequence input)
        {
            throw new NotImplementedException("Cannot explain N1QL queries in memory");
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new ExplainResultOperator();
        }

        public override IStreamedDataInfo GetOutputDataInfo(IStreamedDataInfo inputInfo)
        {
            if (inputInfo == null)
            {
                throw new ArgumentNullException("inputInfo");
            }

            var sequenceInfo = inputInfo as StreamedSequenceInfo;
            if (sequenceInfo == null)
            {
                throw new ArgumentException(string.Format("Parameter 'inputInfo' has unexpected type '{0}'.", inputInfo.GetType()));
            }

            return GetOutputDataInfo(sequenceInfo);
        }

        private StreamedValueInfo GetOutputDataInfo(StreamedSequenceInfo sequenceInfo)
        {
            return new StreamedScalarValueInfo(typeof(object));
        }

        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
            //no parameters so just ignore this
            //throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Explain()";
        }
    }
}
