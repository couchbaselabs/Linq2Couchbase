using Couchbase.Linq.Filters;

#nullable enable

namespace Couchbase.Linq
{
    /// <summary>
    /// Configuration for Linq2Couchbase.
    /// </summary>
    /// <remarks>
    /// Can be configured during calls to <see cref="LinqClusterOptionsExtensions.AddLinq(ClusterOptions)"/>.
    /// </remarks>
    public class CouchbaseLinqConfiguration
    {
        /// <summary>
        /// A <see cref="DocumentFilterManager"/> which registers various extensions that control how
        /// POCOs are filtered from the bucket. By default, POCOs are inspected for <see cref="DocumentFilterAttribute"/>
        /// attributes, such as the <see cref="DocumentTypeFilterAttribute"/>.
        /// </summary>
        public DocumentFilterManager DocumentFilterManager { get; } = new DocumentFilterManager();
    }
}
