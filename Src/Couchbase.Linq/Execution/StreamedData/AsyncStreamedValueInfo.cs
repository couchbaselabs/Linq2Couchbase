using System;
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
        public abstract IStreamedData ExecuteQueryModel(QueryModel queryModel, IQueryExecutor executor);

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

        public sealed override bool Equals(object obj)
        {
            return Equals(obj as IStreamedDataInfo);
        }

        public virtual bool Equals(IStreamedDataInfo obj)
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
