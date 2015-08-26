using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
