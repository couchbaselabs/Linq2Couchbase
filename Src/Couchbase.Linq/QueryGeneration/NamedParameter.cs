using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration
{
    public sealed class NamedParameter
    {
        public string Name { get; set; }

        public object Value { get; set; }
    }
}
