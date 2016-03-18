using System;
using System.Linq.Expressions;
using Couchbase.Linq.QueryGeneration;
using Couchbase.N1QL;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Operators
{
    /// <summary>
    /// When present on a query model, indicates that the raw query string should be returned in a
    /// <see cref="LinqQueryRequest"/>.
    /// </summary>
    internal class ToQueryRequestResultOperator : ValueFromSequenceResultOperatorBase
    {
        public override StreamedValue ExecuteInMemory<T>(StreamedSequence input)
        {
            throw new NotImplementedException("Cannot create N1QL query requests in memory");
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new ToQueryRequestResultOperator();
        }

        public override IStreamedDataInfo GetOutputDataInfo(IStreamedDataInfo inputInfo)
        {
            var returnDefaultWhenEmpty = false;

            var inputAsStreamedSingle = inputInfo as StreamedSingleValueInfo;
            if (inputAsStreamedSingle != null)
            {
                // If the incoming stream is a StreamedSingleValueInfo (i.e. .First() or .Single()),
                // retain the ReturnDefaultWhenEmpty property.  This will cause it to be applied to
                // the created LinqQueryRequest.

                returnDefaultWhenEmpty = inputAsStreamedSingle.ReturnDefaultWhenEmpty;
            }

            return new LinqQueryRequestDataInfo(returnDefaultWhenEmpty);
        }

        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
            //no parameters so just ignore this
        }

        public override string ToString()
        {
            return "ToQueryRequest()";
        }
    }
}
