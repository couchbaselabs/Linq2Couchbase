using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.Proxies
{
    internal interface ITrackedDocumentNodeCallback
    {
        void DocumentModified();
    }
}
