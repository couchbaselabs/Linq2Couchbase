using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Configuration.Client;

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
            return new ClientConfiguration();
        }
    }
}
