using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;

namespace Couchbase.Linq.QueryGeneration
{
    public sealed class CommandData
    {
        private readonly QueryPartsAggregator _aggregator;
        private readonly IBucket _bucket;

        public CommandData(QueryPartsAggregator aggregator, IBucket bucket)
        {
            _aggregator = aggregator;
            _bucket = bucket;
        }

        public string CreateQuery(IBucket bucket)
        {
            var query = _aggregator.BuildN1QlQuery();
            return query;
        }
    }
}
