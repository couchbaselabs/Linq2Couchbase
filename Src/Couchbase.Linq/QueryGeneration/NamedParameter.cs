namespace Couchbase.Linq.QueryGeneration
{
    public sealed class NamedParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}