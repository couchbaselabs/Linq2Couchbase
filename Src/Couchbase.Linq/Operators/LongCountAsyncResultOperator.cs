﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Couchbase.Linq.Execution.StreamedData;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Operators
{
    /// <summary>
    /// Result operator for LongCountAsync.
    /// </summary>
    internal class LongCountAsyncResultOperator : AsyncValueFromSequenceResultOperatorBase
    {
        /// <inheritdoc />
        public override ResultOperatorBase Clone(CloneContext cloneContext) =>
            new LongCountAsyncResultOperator();

        /// <inheritdoc />
        public override AsyncStreamedValue ExecuteInMemory<T>(StreamedSequence input)
        {
            var sequence = input.GetTypedSequence<T>();
            var result = sequence.LongCount();
            return new AsyncStreamedValue(Task.FromResult(result), GetOutputDataInfo(input.DataInfo));
        }

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

        protected AsyncStreamedValueInfo GetOutputDataInfo(StreamedSequenceInfo streamedSequenceInfo)
        {
            if (streamedSequenceInfo == null)
            {
                throw new ArgumentNullException(nameof(streamedSequenceInfo));
            }

            return new AsyncStreamedScalarValueInfo(typeof(Task<long>));
        }

        /// <inheritdoc />
        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
        }
    }
}
