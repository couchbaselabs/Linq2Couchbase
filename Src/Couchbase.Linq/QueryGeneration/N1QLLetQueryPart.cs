namespace Couchbase.Linq.QueryGeneration
{
    /// <summary>
    /// Represents an item in the LET part of a N1QL query
    /// </summary>
    internal class N1QlLetQueryPart
    {
        /// <summary>
        /// Name of the value being assigned
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Expression assigned to the item name
        /// </summary>
        public string Value { get; set; }
    }
}
