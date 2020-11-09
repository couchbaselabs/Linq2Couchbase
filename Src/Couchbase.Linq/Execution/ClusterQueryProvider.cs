using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Couchbase.Linq.Execution.StreamedData;
using Remotion.Linq;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing.Structure;

namespace Couchbase.Linq.Execution
{
    internal class ClusterQueryProvider : QueryProviderBase, IAsyncQueryProvider
    {
        public static readonly MethodInfo ExecuteAsyncMethod =
            typeof(IAsyncQueryExecutor).GetMethod(nameof(IAsyncQueryExecutor.ExecuteCollectionAsync),
                new[] {typeof(QueryModel), typeof(CancellationToken)})
            ?? throw new MissingMemberException();

        public ClusterQueryProvider(IQueryParser queryParser, IAsyncQueryExecutor executor)
            : base(queryParser, executor)
        {
        }

        public override IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return (IQueryable<T>) Activator.CreateInstance(
                typeof(CollectionQueryable<>).MakeGenericType(typeof(T)),
                this, expression);
        }

        public T ExecuteAsync<T>(Expression expression, CancellationToken cancellationToken = default)
        {
            var queryModel = GenerateQueryModel(expression);

            var streamedDataInfo = queryModel.GetOutputDataInfo();

            if (streamedDataInfo is StreamedSequenceInfo sequence)
            {
                var executeAsyncMethod = ExecuteAsyncMethod.MakeGenericMethod(sequence.ResultItemType);

                return (T) executeAsyncMethod.Invoke(Executor, new object[] {queryModel, cancellationToken});
            }
            else if (streamedDataInfo is AsyncStreamedValueInfo streamedValue)
            {
                return (T) streamedValue.ExecuteQueryModel(queryModel, Executor, cancellationToken).Value;
            }
            else
            {
                return (T) streamedDataInfo.ExecuteQueryModel(queryModel, Executor).Value;
            }
        }
    }
}
