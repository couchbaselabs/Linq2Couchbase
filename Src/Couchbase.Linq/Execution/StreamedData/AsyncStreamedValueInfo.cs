using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Remotion.Linq;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Execution.StreamedData
{
    /// <summary>
    /// Provides <see cref="IStreamedDataInfo"/> for <see cref="AsyncStreamedValue"/>, which is returning
    /// a single value asynchronously.
    /// </summary>
    internal abstract class AsyncStreamedValueInfo : IStreamedDataInfo
    {
        private static readonly MethodInfo ExecuteMethod =
            typeof(AsyncStreamedValueInfo).GetMethod(nameof(ExecuteQueryModelAsync),
                new[] {typeof(QueryModel), typeof(IAsyncQueryExecutor), typeof(CancellationToken)})!;

        /// <inheritdoc />
        /// <remarks>
        /// This will always be a Task&lt;T&gt;, where T is <see cref="InternalType"/>.
        /// </remarks>
        public Type DataType { get; }

        /// <summary>
        /// Type being wrapped by the Task.
        /// </summary>
        public Type InternalType { get; }

        protected AsyncStreamedValueInfo(Type dataType)
        {
            DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));

            if (!dataType.IsGenericType || dataType.GetGenericTypeDefinition() != typeof(Task<>))
            {
                throw new ArgumentException($"{nameof(dataType)} must be a Task<T>");
            }

            InternalType = dataType.GetGenericArguments()[0];
        }

        /// <inheritdoc />
        public IStreamedData ExecuteQueryModel(QueryModel queryModel, IQueryExecutor executor) =>
            ExecuteQueryModel(queryModel, executor, default);

        /// <inheritdoc cref="IStreamedDataInfo.ExecuteQueryModel" />
        public IStreamedData ExecuteQueryModel(QueryModel queryModel, IQueryExecutor executor, CancellationToken cancellationToken)
        {
            if (queryModel == null)
            {
                throw new ArgumentNullException(nameof(queryModel));
            }
            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }
            if (!(executor is IAsyncQueryExecutor asyncExecutor))
            {
                throw new ArgumentException($"{nameof(executor)} must implement {typeof(IAsyncQueryExecutor)} for asynchronous queries.");
            }

            var executeMethod = ExecuteMethod.MakeGenericMethod(InternalType);

            // wrap executeMethod into a delegate instead of calling Invoke in order to allow for exceptions that are bubbled up correctly
            var func = (Func<QueryModel, IAsyncQueryExecutor, CancellationToken, Task>)
                executeMethod.CreateDelegate(typeof (Func<QueryModel, IAsyncQueryExecutor, CancellationToken, Task>), this);
            var result = func(queryModel, asyncExecutor, cancellationToken);

            return new AsyncStreamedValue(result, this);
        }

        public abstract Task<T?> ExecuteQueryModelAsync<T>(QueryModel queryModel, IAsyncQueryExecutor executor,
            CancellationToken cancellationToken = default);

        protected abstract AsyncStreamedValueInfo CloneWithNewDataType (Type dataType);

        /// <inheritdoc />
        public virtual IStreamedDataInfo AdjustDataType (Type dataType)
        {
            if (dataType == null)
            {
                throw new ArgumentNullException(nameof(dataType));
            }

            if (!dataType.IsAssignableFrom(DataType))
            {
                throw new ArgumentException(
                    $"'{dataType}' cannot be used as the new data type for a value of type '{DataType}'.",
                    nameof(dataType));
            }

            return CloneWithNewDataType(dataType);
        }

        public sealed override bool Equals(object? obj)
        {
            return Equals(obj as IStreamedDataInfo);
        }

        public virtual bool Equals(IStreamedDataInfo? obj)
        {
            if (obj == null)
                return false;

            if (GetType () != obj.GetType ())
                return false;

            var other = (StreamedValueInfo) obj;
            return DataType == other.DataType;
        }

        public override int GetHashCode()
        {
            return DataType.GetHashCode();
        }
    }
}
