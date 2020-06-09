using System;
using System.Threading.Tasks;
using Couchbase.Linq.Execution.StreamedData;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Operators
{
    /// <summary>
    /// Abstract base for result operators which return a single result asynchronously, such as <see cref="FirstAsyncResultOperator"/>.
    /// </summary>
    internal abstract class ChoiceAsyncResultOperatorBase : AsyncValueFromSequenceResultOperatorBase
    {
        /// <summary>
        /// If true, an empty collection should return the default value.
        /// </summary>
        public bool ReturnDefaultWhenEmpty { get; set; }

        protected ChoiceAsyncResultOperatorBase(bool returnDefaultWhenEmpty)
        {
            ReturnDefaultWhenEmpty = returnDefaultWhenEmpty;
        }

        public sealed override IStreamedDataInfo GetOutputDataInfo(IStreamedDataInfo inputInfo)
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

        protected AsyncStreamedValueInfo GetOutputDataInfo(StreamedSequenceInfo streamedSequenceInfo)
        {
            if (streamedSequenceInfo == null)
            {
                throw new ArgumentNullException(nameof(streamedSequenceInfo));
            }

            var taskType = typeof(Task<>).MakeGenericType(streamedSequenceInfo.ResultItemType);

            return new AsyncStreamedSingleValueInfo(taskType, ReturnDefaultWhenEmpty);
        }
    }
}
