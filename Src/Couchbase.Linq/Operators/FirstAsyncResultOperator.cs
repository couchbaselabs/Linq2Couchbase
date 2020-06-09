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
    /// Result operator for FirstAsync and FirstOrDefaultAsync.
    /// </summary>
    internal class FirstAsyncResultOperator : ChoiceAsyncResultOperatorBase
    {
        public FirstAsyncResultOperator (bool returnDefaultWhenEmpty)
            : base (returnDefaultWhenEmpty)
        {
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext) =>
            new FirstAsyncResultOperator(ReturnDefaultWhenEmpty);

        public override AsyncStreamedValue ExecuteInMemory<T>(StreamedSequence input)
        {
            var sequence = input.GetTypedSequence<T>();
            T result = ReturnDefaultWhenEmpty ? sequence.FirstOrDefault() : sequence.First();
            return new AsyncStreamedValue (Task.FromResult(result), GetOutputDataInfo (input.DataInfo));
        }

        /// <inheritdoc />
        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
        }

        public override string ToString() =>
            ReturnDefaultWhenEmpty
                ? "FirstOrDefaultAsync()"
                : "FirstAsync()";
    }
}
