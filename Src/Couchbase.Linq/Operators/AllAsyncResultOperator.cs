using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Couchbase.Linq.Execution.StreamedData;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Operators
{
    /// <summary>
    /// Result operator for AllAsync.
    /// </summary>
    internal class AllAsyncResultOperator : AsyncValueFromSequenceResultOperatorBase
    {
        public Expression Predicate { get; private set; }

        public AllAsyncResultOperator(Expression predicate)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <inheritdoc />
        public override ResultOperatorBase Clone(CloneContext cloneContext) =>
            new AllAsyncResultOperator(Predicate);

        /// <inheritdoc />
        public override AsyncStreamedValue ExecuteInMemory<T>(StreamedSequence input)
        {
            var sequence = input.GetTypedSequence<T>();

            var predicateLambda = ReverseResolvingExpressionVisitor.ReverseResolve(
                input.DataInfo.ItemExpression, Predicate);
            var predicate = (Func<T, bool>) predicateLambda.Compile();

            var result = sequence.All(predicate);
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

            return new AsyncStreamedScalarValueInfo(typeof(Task<bool>));
        }

        /// <inheritdoc />
        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
            if (transformation == null)
            {
                throw new ArgumentNullException(nameof(transformation));
            }

            Predicate = transformation(Predicate);
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"AllAsync({Predicate})";
    }
}
