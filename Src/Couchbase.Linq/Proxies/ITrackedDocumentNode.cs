using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.Proxies
{
    internal interface ITrackedDocumentNode
    {
        bool IsDeserializing { get; set; }
        bool IsDirty { get; set; }

        void RegisterChangeTracking(ITrackedDocumentNodeCallback callback);
        void UnregisterChangeTracking(ITrackedDocumentNodeCallback callback);

        /// <summary>
        /// Clears IsDeserializing and IsDirty on this document and all child documents.
        /// Does nothing if IsDeserialization is already false to prevent accidental infinite recursion.
        /// </summary>
        void ClearStatus();
    }
}
