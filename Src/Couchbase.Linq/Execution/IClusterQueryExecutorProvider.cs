namespace Couchbase.Linq.Execution
{
    /// <summary>
    /// Provides access to an <see cref="IClusterQueryExecutor"/>.
    /// </summary>
    internal interface IClusterQueryExecutorProvider
    {
        /// <summary>
        /// Get the <see cref="IClusterQueryExecutor"/>.
        /// </summary>
        IClusterQueryExecutor ClusterQueryExecutor { get; }
    }
}
