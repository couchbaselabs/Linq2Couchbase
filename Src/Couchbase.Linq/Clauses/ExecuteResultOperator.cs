using System;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Clauses
{
    internal class ExecuteResultOperator : Remotion.Linq.Clauses.ResultOperatorBase
    {
        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new ExecuteResultOperator();
        }

        public override IStreamedData ExecuteInMemory(IStreamedData input)
        {
            return new StreamedValue(null, new StreamedSingleValueInfo(typeof(void), true));
        }

        public override IStreamedDataInfo GetOutputDataInfo(IStreamedDataInfo inputInfo)
        {
            return new StreamedScalarValueInfo(typeof(void));
        }

        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
        }
    }
}