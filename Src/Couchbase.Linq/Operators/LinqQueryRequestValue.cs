using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Operators
{
    /// <summary>
    /// Implementation of <see cref="IStreamedData"/> for a query where <see cref="ToQueryRequestResultOperator"/>
    /// was applied.  Value will be a <see cref="LinqQueryRequest"/>.
    /// </summary>
    internal class LinqQueryRequestValue : IStreamedData
    {
        public IStreamedDataInfo DataInfo { get; private set; }

        public object Value { get; private set; }

        public LinqQueryRequestValue(LinqQueryRequest value, LinqQueryRequestDataInfo linqQueryRequestInfo)
        {
            if (linqQueryRequestInfo == null)
            {
                throw new ArgumentNullException("linqQueryRequestInfo");
            }

            Value = value;
            DataInfo = linqQueryRequestInfo;
        }
    }
}
