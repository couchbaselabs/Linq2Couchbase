using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Remotion.Linq;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Execution.StreamedData
{
    /// <summary>
    /// Version of <see cref="AsyncStreamedValueInfo"/> intended for scalar result objects, for example
    /// from CountAsync.
    /// </summary>
    internal class AsyncStreamedScalarValueInfo : AsyncStreamedValueInfo
    {
        /// <summary>
        /// Creates a new AsyncStreamedScalarValueInfo.
        /// </summary>
        /// <param name="dataType">Data type returned by the <see cref="IStreamedData"/>. Must be of type <see cref="Task{T}"/>.</param>
        public AsyncStreamedScalarValueInfo(Type dataType)
            : base(dataType)
        {
        }

        /// <inheritdoc />
        protected override AsyncStreamedValueInfo CloneWithNewDataType(Type dataType) =>
            new AsyncStreamedScalarValueInfo(dataType);

        /// <inheritdoc />
#pragma warning disable CS8609 // Nullability of reference types in return type doesn't match overridden member.
        public override Task<T> ExecuteQueryModelAsync<T>(QueryModel queryModel, IAsyncQueryExecutor executor,
#pragma warning restore CS8609 // Nullability of reference types in return type doesn't match overridden member.
            CancellationToken cancellationToken = default)
            where T : default
        {
            if (queryModel == null)
            {
                throw new ArgumentNullException(nameof(queryModel));
            }

            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }

            return executor.ExecuteScalarAsync<T>(queryModel, cancellationToken);
        }
    }
}
