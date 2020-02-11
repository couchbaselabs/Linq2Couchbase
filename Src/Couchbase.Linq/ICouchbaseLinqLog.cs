namespace Couchbase.Linq
{
    public interface ICouchbaseLinqLog
    {
        void Debug(string format, params object[] args);
        void Info(string message);
    }
}
