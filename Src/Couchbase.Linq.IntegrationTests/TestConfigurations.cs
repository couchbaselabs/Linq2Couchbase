using Microsoft.Extensions.Configuration;

namespace Couchbase.Linq.IntegrationTests
{
    public class TestConfigurations
    {
        private static IConfigurationRoot _jsonConfiguration;

        public static ClusterOptions DefaultConfig()
        {
            return DefaultLocalhostConfig();
        }

        private static ClusterOptions DefaultLocalhostConfig()
        {
            EnsureConfigurationLoaded();

            var options = new ClusterOptions();
            _jsonConfiguration.GetSection("couchbase").Bind(options);

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
