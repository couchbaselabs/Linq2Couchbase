namespace Couchbase.Linq.QueryGeneration
{
    internal sealed class NamedParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}