using System;
using Couchbase.Linq.IntegrationTests.Documents;

namespace Couchbase.Linq.IntegrationTests
{
    public class TravelSample : BucketContext
    {
        public IDocumentSet<Airline> Airlines { get; set; }
        public IDocumentSet<RouteInCollection> Routes { get; set; }

        public TravelSample(IBucket bucket)
            : base(bucket)
        {
        }
    }
}
