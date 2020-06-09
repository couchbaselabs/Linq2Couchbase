using System;
using System.Reflection;
using Couchbase.Linq.Execution.StreamedData;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Operators
{
    /// <summary>
    /// Abstract base for result operators which return their result asynchronously.
    /// </summary>
    internal abstract class AsyncValueFromSequenceResultOperatorBase : ResultOperatorBase
    {
        private static readonly MethodInfo ExecuteMethod = typeof(AsyncValueFromSequenceResultOperatorBase)
            .GetMethod(nameof(ExecuteInMemory), new[] {typeof(StreamedSequence)});

        public abstract AsyncStreamedValue ExecuteInMemory<T>(StreamedSequence sequence);

        /// <inheritdoc />
        public sealed override IStreamedData ExecuteInMemory(IStreamedData input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (!(input is StreamedSequence streamedSequence))
            {
                throw new ArgumentException($"{nameof(input)} must be of type {typeof(StreamedSequence)}");
            }

            var executeMethod = ExecuteMethod.MakeGenericMethod(streamedSequence.DataInfo.ResultItemType);
            return (AsyncStreamedValue) InvokeExecuteMethod(executeMethod, streamedSequence);
        }
    }
}
