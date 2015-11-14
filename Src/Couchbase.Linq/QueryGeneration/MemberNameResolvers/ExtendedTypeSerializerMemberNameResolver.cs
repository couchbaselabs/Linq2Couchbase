using System;
using System.Linq;
using System.Reflection;
using Couchbase.Core.Serialization;
using Newtonsoft.Json.Serialization;

namespace Couchbase.Linq.QueryGeneration.MemberNameResolvers
{
    /// <summary>
    /// Implementation of <see cref="IMemberNameResolver"/> which uses an <see cref="IExtendedTypeSerializer"/>
    /// to resolve member names.
    /// </summary>
    internal class ExtendedTypeSerializerMemberNameResolver : IMemberNameResolver
    {
        private readonly IExtendedTypeSerializer _serializer;

        public ExtendedTypeSerializerMemberNameResolver(IExtendedTypeSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            _serializer = serializer;
        }

        public bool TryResolveMemberName(MemberInfo member, out string memberName)
        {
            memberName = null;

            if (member == null)
                return false;

            memberName = _serializer.GetMemberName(member);

            return memberName != null;
        }
    }
}