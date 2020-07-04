using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Couchbase.Linq.Execution.StreamedData;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Operators
{
    /// <summary>
    /// Result operator for ExplainAsync.
    /// </summary>
    internal class ExplainAsyncResultOperator : AsyncValueFromSequenceResultOperatorBase
    {
        /// <inheritdoc />
        public override ResultOperatorBase Clone(CloneContext cloneContext) =>
            new ExplainAsyncResultOperator();

        /// <inheritdoc />
        public override AsyncStreamedValue ExecuteInMemory<T>(StreamedSequence input) =>
            throw new NotImplementedException("Cannot explain N1QL queries in memory");

        /// <inheritdoc />
        public override IStreamedDataInfo GetOutputDataInfo(IStreamedDataInfo inputInfo)
        {
            if (inputInfo == null)
            {
                throw new ArgumentNullException(nameof(inputInfo));
            }
            if (!(inputInfo is StreamedSequenceInfo streamedSequenceInfo))
            {
                throw new ArgumentException($"{nameof(inputInfo)} must be of type {typeof(StreamedSequenceInfo)}");
            }

            return GetOutputDataInfo(streamedSequenceInfo);
        }

        private AsyncStreamedValueInfo GetOutputDataInfo(StreamedSequenceInfo streamedSequenceInfo)
        {
            if (streamedSequenceInfo == null)
            {
                throw new ArgumentNullException(nameof(streamedSequenceInfo));
            }

            return new AsyncStreamedScalarValueInfo(typeof(Task<dynamic>));
        }

        /// <inheritdoc />
        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "ExplainAsync()";
        }
    }
}
