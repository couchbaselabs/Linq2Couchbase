using System;

namespace Couchbase.Linq.Serialization
{
    /// <summary>
    /// Indicates how a <see cref="DateTime"/> property is serialized. This does not affect
    /// the actual serialization or deserialization, a JsonConverter or similar method
    /// should be used. This setting simply informs the query generator what format to expect.
    /// </summary>
    public enum DateTimeSerializationFormat
    {
        Iso8601,
        UnixMilliseconds
    }
}
