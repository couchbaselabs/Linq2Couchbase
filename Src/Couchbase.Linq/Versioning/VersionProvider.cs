namespace Couchbase.Linq.Versioning
{
    /// <summary>
    /// Singleton for the <see cref="IVersionProvider"/> implementation in use for query generation.
    /// </summary>
    internal static class VersionProvider
    {
        static VersionProvider()
        {
            Current = new DefaultVersionProvider();
        }

        /// <summary>
        /// Singleton for the <see cref="IVersionProvider"/> implementation in use for query generation.
        /// </summary>
        public static IVersionProvider Current { get; set; }
    }
}
