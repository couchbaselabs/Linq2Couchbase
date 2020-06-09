using System;
using System.Reflection;
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
        private static readonly MethodInfo ExecuteMethod =
            typeof(AsyncStreamedSingleValueInfo).GetMethod(nameof(ExecuteSingleQueryModel),
                new[] {typeof(QueryModel), typeof(IClusterQueryExecutor)});

        public bool ReturnDefaultWhenEmpty { get; }

        public AsyncStreamedSingleValueInfo(Type dataType, bool returnDefaultWhenEmpty)
            : base(dataType)
        {
            ReturnDefaultWhenEmpty = returnDefaultWhenEmpty;
        }

        public override IStreamedData ExecuteQueryModel(QueryModel queryModel, IQueryExecutor executor)
        {
            if (queryModel == null)
            {
                throw new ArgumentNullException(nameof(queryModel));
            }
            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }
            if (!(executor is IClusterQueryExecutor asyncExecutor))
            {
                throw new ArgumentException($"{nameof(executor)} must implement {typeof(IClusterQueryExecutor)} for asynchronous queries.");
            }

            var executeMethod = ExecuteMethod.MakeGenericMethod(InternalType);

            // wrap executeMethod into a delegate instead of calling Invoke in order to allow for exceptions that are bubbled up correctly
            var func = (Func<QueryModel, IClusterQueryExecutor, object>) executeMethod.CreateDelegate (typeof (Func<QueryModel, IClusterQueryExecutor, object>), this);
            var result = func(queryModel, asyncExecutor);

            return new AsyncStreamedValue(result, this);
        }

        protected override AsyncStreamedValueInfo CloneWithNewDataType(Type dataType) =>
            new AsyncStreamedSingleValueInfo (dataType, ReturnDefaultWhenEmpty);

        public object ExecuteSingleQueryModel<T>(QueryModel queryModel, IClusterQueryExecutor executor)
        {
            if (queryModel == null)
            {
                throw new ArgumentNullException(nameof(queryModel));
            }
            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }

            return executor.ExecuteSingleAsync<T>(queryModel, ReturnDefaultWhenEmpty);
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
