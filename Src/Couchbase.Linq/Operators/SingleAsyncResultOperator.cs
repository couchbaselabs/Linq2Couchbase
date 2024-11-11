using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Couchbase.Linq.Execution.StreamedData;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Operators
{
    /// <summary>
    /// Result operator for SingleAsync and SingleOrDefaultAsync.
    /// </summary>
    internal class SingleAsyncResultOperator : ChoiceAsyncResultOperatorBase
    {
        public SingleAsyncResultOperator (bool returnDefaultWhenEmpty)
            : base (returnDefaultWhenEmpty)
        {
        }

        /// <inheritdoc />
        public override ResultOperatorBase Clone(CloneContext cloneContext) =>
            new SingleAsyncResultOperator(ReturnDefaultWhenEmpty);

        /// <inheritdoc />
        public override AsyncStreamedValue? ExecuteInMemory<T>(StreamedSequence input)
        {
            var sequence = input.GetTypedSequence<T>();
            T? result = ReturnDefaultWhenEmpty ? sequence.SingleOrDefault() : sequence.Single();
            return new AsyncStreamedValue (Task.FromResult(result), GetOutputDataInfo (input.DataInfo));
        }

        /// <inheritdoc />
        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
        }

        /// <inheritdoc />
        public override string ToString() =>
            ReturnDefaultWhenEmpty
                ? "SingleOrDefaultAsync()"
                : "SingleAsync()";
    }
}
