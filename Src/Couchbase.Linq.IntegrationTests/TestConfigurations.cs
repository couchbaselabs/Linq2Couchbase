using System;
using Microsoft.Extensions.Configuration;

namespace Couchbase.Linq.IntegrationTests
{
    public class TestConfigurations
    {
        private static IConfigurationRoot _jsonConfiguration;

        public static ClusterOptions DefaultConfig(Action<CouchbaseLinqConfiguration> setupAction = null)
        {
            return DefaultLocalhostConfig(setupAction);
        }

        private static ClusterOptions DefaultLocalhostConfig(Action<CouchbaseLinqConfiguration> setupAction = null)
        {
            EnsureConfigurationLoaded();

            var options = new ClusterOptions();
            _jsonConfiguration.GetSection("couchbase").Bind(options);

            options.AddLinq(setupAction);

            return options;
        }

        private static void EnsureConfigurationLoaded()
        {
            if (_jsonConfiguration == null)
            {
                var builder = new ConfigurationBuilder();
                builder.AddJsonFile("config.json");
                _jsonConfiguration = builder.Build();
            }
        }
    }
}
