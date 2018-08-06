using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Moq;

namespace Couchbase.Linq.UnitTests
{
    internal class QueryFactory
    {
        public static IQueryable<T> Queryable<T>(IBucket bucket)
        {
            var configuration = new ClientConfiguration();

            var bucketContext = new Mock<IBucketContext>();
            bucketContext.SetupGet(p => p.Bucket).Returns(bucket);
            bucketContext.SetupGet(p => p.Configuration).Returns(configuration);

            //TODO refactor so ClientConfiguration is injectable
            return new BucketQueryable<T>(bucket, configuration, bucketContext.Object);
        }
    }
}