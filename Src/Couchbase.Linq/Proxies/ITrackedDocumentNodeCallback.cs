
namespace Couchbase.Linq.Proxies
{
    internal interface ITrackedDocumentNodeCallback
    {
        void DocumentModified(ITrackedDocumentNode mutatedDocument);
    }
}
