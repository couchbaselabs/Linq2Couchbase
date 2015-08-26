using System.Configuration;
using Couchbase.Configuration.Client;
using Couchbase.Configuration.Client.Providers;

namespace Couchbase.Linq.Tests
{
    public class TestConfigurations
    {
        public static ClientConfiguration DefaultConfig()
        {
            return DefaultLocalhostConfig();
        }

        private static ClientConfiguration DefaultLocalhostConfig()
        {
            var section = (CouchbaseClientSection)ConfigurationManager.GetSection("couchbaseClients/couchbase");
            return new ClientConfiguration(section);
        }
    }
}
