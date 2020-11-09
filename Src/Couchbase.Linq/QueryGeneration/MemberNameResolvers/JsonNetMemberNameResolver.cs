using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Couchbase.Linq.QueryGeneration.MemberNameResolvers
{
    /// <summary>
    /// Implementation of <see cref="IMemberNameResolver"/> which uses a Newtonsoft.Json
    /// <see cref="IContractResolver"/> to resolve member names.
    /// </summary>
    /// <remarks>
    /// Used for backwards compatibility with older implementations of <see cref="Couchbase.Core.IO.Serializers.ITypeSerializer"/>
    /// which don't have a GetMemberName implementation like <see cref="Couchbase.Core.IO.Serializers.IExtendedTypeSerializer"/>.
    /// </remarks>
    internal class JsonNetMemberNameResolver : IMemberNameResolver
    {
        private readonly IContractResolver _contractResolver;

        public JsonNetMemberNameResolver(IContractResolver contractResolver)
        {
            if (contractResolver == null)
            {
                throw new ArgumentNullException("contractResolver");
            }

            _contractResolver = contractResolver;
        }

        public bool TryResolveMemberName(MemberInfo member, out string memberName)
        {
            memberName = null;

            if (member == null)
                return false;

            var contract = _contractResolver.ResolveContract(member.DeclaringType);

            if (contract.GetType() == typeof (JsonObjectContract) &&
                ((JsonObjectContract) contract).Properties.Any(p => p.UnderlyingName == member.Name && !p.Ignored))
            {
                memberName =
                    ((JsonObjectContract) contract).Properties.First(p => p.UnderlyingName == member.Name).PropertyName;
                return true;
            }

            return false;
        }
    }
}