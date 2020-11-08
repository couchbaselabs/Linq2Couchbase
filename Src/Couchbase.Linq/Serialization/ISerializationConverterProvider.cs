using System.Reflection;
using Couchbase.Core.IO.Serializers;

namespace Couchbase.Linq.Serialization
{
    /// <summary>
    /// Extension applied to <see cref="ITypeSerializer"/> implementations to provide
    /// information about how members are serialized.
    /// </summary>
    public interface ISerializationConverterProvider
    {
        /// <summary>
        /// Provides information about how a member is converted when it is serialized.
        /// </summary>
        /// <param name="member">Member being serialized or deserialized.</param>
        /// <returns>A <see cref="ISerializationConverter"/> which can be used to affect N1QL query generation, or null if none.</returns>
        /// <remarks>
        /// Should implement an internal cache for performance, as this method will be
        /// called repeatedly for the same member.
        /// </remarks>
        ISerializationConverter? GetSerializationConverter(MemberInfo member);
    }
}
