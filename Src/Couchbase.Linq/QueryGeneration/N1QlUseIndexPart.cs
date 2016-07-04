using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration
{
    internal class N1QlUseIndexPart
    {
        /// <summary>
        /// Name of the index being used, already escaped.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Type of the index being used.  Should be alphanumeric only.
        /// </summary>
        public string IndexType { get; set; }
    }
}
