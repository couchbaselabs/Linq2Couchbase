using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;

namespace Couchbase.Linq
{
    public interface IDbContext : IBucketQueryable
    {
        ICluster Cluster { get; }

        ClientConfiguration Configuration { get; }

        IQueryable<T> Query<T>();
    }
}
