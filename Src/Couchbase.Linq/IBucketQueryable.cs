namespace Couchbase.Linq
{
    /// <summary>
    /// Used to provide the bucket name to the query generator
    /// </summary>
    public interface IBucketQueryable
    {
        string BucketName { get; }
    }
}
