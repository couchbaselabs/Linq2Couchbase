using System;
using System.Threading;
using System.Threading.Tasks;
using Remotion.Linq;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Execution.StreamedData
{
    /// <summary>
    /// Version of <see cref="AsyncStreamedValueInfo"/> intended for single result objects, for example
    /// from FirstAsync or SingleAsync.
    /// </summary>
    internal class AsyncStreamedSingleValueInfo : AsyncStreamedValueInfo
    {
        public bool ReturnDefaultWhenEmpty { get; }

        public AsyncStreamedSingleValueInfo(Type dataType, bool returnDefaultWhenEmpty)
            : base(dataType)
        {
            ReturnDefaultWhenEmpty = returnDefaultWhenEmpty;
        }

        protected override AsyncStreamedValueInfo CloneWithNewDataType(Type dataType) =>
            new AsyncStreamedSingleValueInfo (dataType, ReturnDefaultWhenEmpty);

        public override Task<T> ExecuteQueryModelAsync<T>(QueryModel queryModel, IAsyncQueryExecutor executor,
            CancellationToken cancellationToken = default)
        {
            if (queryModel == null)
            {
                throw new ArgumentNullException(nameof(queryModel));
            }

            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }

            return executor.ExecuteSingleAsync<T>(queryModel, ReturnDefaultWhenEmpty, cancellationToken);
        }

        // ReSharper disable PossibleNullReferenceException
        public override bool Equals(IStreamedDataInfo obj) =>
            base.Equals(obj) &&
            ((AsyncStreamedSingleValueInfo) obj).ReturnDefaultWhenEmpty == ReturnDefaultWhenEmpty;
        // ReSharper restore PossibleNullReferenceException

        public override int GetHashCode() =>
            base.GetHashCode() ^ ReturnDefaultWhenEmpty.GetHashCode();
    }
}
