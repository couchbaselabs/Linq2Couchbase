using System.Threading.Tasks;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Execution.StreamedData
{
    /// <summary>
    /// Implementation of <see cref="IStreamedData"/> intended for single value results
    /// being returned asynchronously.
    /// </summary>
    internal class AsyncStreamedValue : IStreamedData
    {
        public object Value { get; }

        public AsyncStreamedValueInfo DataInfo { get; }

        IStreamedDataInfo IStreamedData.DataInfo => DataInfo;

        public AsyncStreamedValue(Task value, AsyncStreamedValueInfo asyncStreamedValueInfo)
        {
            Value = value;
            DataInfo = asyncStreamedValueInfo;
        }
    }
}
