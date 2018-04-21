using System;
using System.Reflection;
using Couchbase.Core.Serialization;

namespace Couchbase.Linq.Serialization
{
    /// <summary>
    /// Extension applied to <see cref="ITypeSerializer"/> implementations to provide
    /// information about how <see cref="DateTime"/> members are serialized.
    /// </summary>
    public interface IDateTimeSerializationFormatProvider
    {
        /// <summary>
        /// Provides information about how a <see cref="DateTime"/> member is serialized.
        /// </summary>
        /// <param name="member">Member being serialized or deserialized.</param>
        /// <returns>The member's <see cref="DateTimeSerializationFormat"/>.</returns>
        /// <remarks>
        /// Should implement an internal cache for performance, as this method will be
        /// called repeatedly for the same member.
        /// </remarks>
        DateTimeSerializationFormat GetDateTimeSerializationFormat(MemberInfo member);
    }
}
