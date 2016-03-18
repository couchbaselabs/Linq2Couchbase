using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.QueryGeneration;
using Remotion.Linq;
using Remotion.Linq.Clauses.StreamedData;

namespace Couchbase.Linq.Operators
{
    /// <summary>
    /// Implementation of <see cref="IStreamedDataInfo"/> for a query where <see cref="ToQueryRequestResultOperator"/>
    /// was applied.
    /// </summary>
    internal class LinqQueryRequestDataInfo : IStreamedDataInfo
    {
        private readonly bool _returnDefaultWhenEmpty;

        public Type DataType
        {
            get { return typeof(LinqQueryRequest); }
        }

        /// <summary>
        /// For queries returning a single result, true indicates that an empty result set should return the default value.
        /// For example, a call to .FirstOrDefault() would set this to true.
        /// </summary>
        public bool ReturnDefaultWhenEmpty
        {
            get { return _returnDefaultWhenEmpty; }
        }

        public LinqQueryRequestDataInfo(bool returnDefaultWhenEmpty)
        {
            _returnDefaultWhenEmpty = returnDefaultWhenEmpty;
        }

        public IStreamedData ExecuteQueryModel(QueryModel queryModel, IQueryExecutor executor)
        {
            if (queryModel == null)
            {
                throw new ArgumentNullException("queryModel");
            }
            if (executor == null)
            {
                throw new ArgumentNullException("executor");
            }

            var result = executor.ExecuteSingle<LinqQueryRequest>(queryModel, false);

            // Pass the value of ReturnDefaultWhenEmpty into the resulting LinqQueryRequest
            result.ReturnDefaultWhenEmpty = ReturnDefaultWhenEmpty;

            return new LinqQueryRequestValue(result, this);
        }

        public IStreamedDataInfo AdjustDataType(Type dataType)
        {
            if (dataType == null)
            {
                throw new ArgumentNullException("dataType");
            }

            if (dataType != typeof(LinqQueryRequest))
            {
                throw new ArgumentException("Invalid dataType for LinqQueryRequestDataInfo.", "dataType");
            }

            return new LinqQueryRequestDataInfo(ReturnDefaultWhenEmpty);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IStreamedDataInfo);
        }

        public virtual bool Equals(IStreamedDataInfo obj)
        {
            var objAsLinqQueryRequestDataInfo = obj as LinqQueryRequestDataInfo;
            if (objAsLinqQueryRequestDataInfo == null)
            {
                return false;
            }

            return ReturnDefaultWhenEmpty == objAsLinqQueryRequestDataInfo.ReturnDefaultWhenEmpty;
        }

        public override int GetHashCode()
        {
            return ReturnDefaultWhenEmpty.GetHashCode();
        }
    }
}
