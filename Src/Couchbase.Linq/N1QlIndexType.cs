using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;

namespace Couchbase.Linq
{
    /// <summary>
    /// Types of indices supported by N1QL query <see cref="QueryExtensions.UseIndex{T}(IQueryable{T}, string, N1QlIndexType)"/>.
    /// </summary>
    public enum N1QlIndexType
    {
        /// <summary>
        /// Global secondary index
        /// </summary>
        Gsi,

        /// <summary>
        /// View index
        /// </summary>
        View
    }
}
