using Couchbase.Configuration.Client;
using Microsoft.Extensions.Configuration;

namespace Couchbase.Linq.IntegrationTests
{
    public class TestConfigurations
    {
        private static IConfigurationRoot _jsonConfiguration;
        private static TestSettings _settings;

        public static TestSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    EnsureConfigurationLoaded();

                    _settings = new TestSettings();
                    _jsonConfiguration.GetSection("testSettings").Bind(_settings);
                }

                return _settings;
            }
        }

        public static ClientConfiguration DefaultConfig()
        {
            return DefaultLocalhostConfig();
        }

        private static ClientConfiguration DefaultLocalhostConfig()
        {
            EnsureConfigurationLoaded();

            var definition = new CouchbaseClientDefinition();
            _jsonConfiguration.GetSection("couchbase").Bind(definition);

            return new ClientConfiguration(definition);
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
