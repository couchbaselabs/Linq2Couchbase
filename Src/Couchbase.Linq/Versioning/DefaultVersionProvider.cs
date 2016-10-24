using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Couchbase.Core;
using Newtonsoft.Json;

namespace Couchbase.Linq.Versioning
{
    /// <summary>
    /// Provides the version for a bucket based on the implementationVersion returned by calls
    /// to http://node-ip:8091/pools.  Caches the results for quick results on future calls.
    /// </summary>
    internal class DefaultVersionProvider : IVersionProvider
    {
        private static readonly Random Random = new Random();
        private static readonly ILog Log = LogManager.GetLogger<DefaultVersionProvider>();

        private readonly ConcurrentDictionary<Uri, Version> _versionsByUri = new ConcurrentDictionary<Uri, Version>();
        private readonly object _lock = new object();

        /// <summary>
        /// Gets the version of the cluster hosting a bucket, using the cluster's
        /// <see cref="Couchbase.Configuration.Client.ClientConfiguration"/>.
        /// </summary>
        /// <param name="bucket">Couchbase bucket.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bucket"/> is null.</exception>
        /// <returns>The version of the cluster hosting this bucket, or 4.0.0 if unable to determine the version.</returns>
        public Version GetVersion(IBucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            var servers = bucket.Configuration.PoolConfiguration.ClientConfiguration.Servers;

            // First check for an existing result
            var version = CacheLookup(servers);
            if (version != null)
            {
                return version;
            }

            var contextCache = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            try
            {
                // Only check one cluster at a time, this prevents multiple lookups during bootstrap
                lock (_lock)
                {
                    // In case the version was received while we were waiting for the lock, check for it again
                    version = CacheLookup(servers);
                    if (version != null)
                    {
                        return version;
                    }

                    foreach (var server in Shuffle(servers))
                    {
                        try
                        {
                            var config = DownloadConfig(server).Result;

                            version = ExtractVersion(config);
                            if (version != null)
                            {
                                CacheStore(servers, version);

                                return version;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.ErrorFormat("Unable to load config from {0}", e, server);
                        }
                    }

                    // No version information could be loaded from any node
                    var fallbackVersion = new Version(4, 0, 0);
                    CacheStore(servers, fallbackVersion);
                    return fallbackVersion;
                }
            }
            finally
            {
                if (contextCache != null)
                {
                    SynchronizationContext.SetSynchronizationContext(contextCache);
                }
            }
        }

        internal virtual async Task<Bootstrap> DownloadConfig(Uri uri)
        {
            try
            {
#if NET45
                using (var handler = new WebRequestHandler())
                {
                    handler.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
#else
                using (var handler = new HttpClientHandler())
                {
                    try
                    {
                        handler.ServerCertificateCustomValidationCallback += ServerCertificateValidationCallback;
                    }
                    catch (NotImplementedException)
                    {
                        Log.Debug("Cannot set ServerCertificateCustomValidationCallback, not supported on this platform");
                    }
#endif
                    using (var httpClient = new HttpClient(handler))
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(5);

                        var response = await httpClient.GetAsync(uri);

                        response.EnsureSuccessStatusCode();

                        return JsonConvert.DeserializeObject<Bootstrap>(await response.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (AggregateException ex)
            {
                // Unwrap the aggregate exception
                throw new HttpRequestException(ex.InnerException.Message, ex.InnerException);
            }
        }

#if NET45
        private static bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
#else
        private static bool ServerCertificateValidationCallback(HttpRequestMessage sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
#endif
        {
            Log.Info(m => m("Validating certificate: {0}", sslPolicyErrors));
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        internal virtual Version ExtractVersion(Bootstrap config)
        {
            if ((config == null) || string.IsNullOrEmpty(config.ImplementationVersion))
            {
                return null;
            }

            var versionStr = config.ImplementationVersion.Split('-')[0];

            Version version;
            if (Version.TryParse(versionStr, out version))
            {
                return version;
            }
            else
            {
                Log.ErrorFormat("Invalid version string {0}", versionStr);
                return null;
            }
        }

        internal virtual List<T> Shuffle<T>(List<T> list)
        {
            list = new List<T>(list);

            var length = list.Count;
            while (length > 1)
            {
                length--;
                var index = Random.Next(length + 1);
                var item = list[index];
                list[index] = list[length];
                list[length] = item;
            }
            return list;
        }

        internal virtual Version CacheLookup(IEnumerable<Uri> servers)
        {
            foreach (var server in servers)
            {
                Version version;
                if (_versionsByUri.TryGetValue(server, out version))
                {
                    return version;
                }
            }

            return null;
        }

        internal virtual void CacheStore(IEnumerable<Uri> servers, Version version)
        {
            foreach (var server in servers)
            {
                _versionsByUri.AddOrUpdate(server, version, (key, oldValue) => version);
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        internal class Bootstrap
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            [JsonProperty("implementationVersion")]
            public string ImplementationVersion { get; set; }
        }
    }
}
