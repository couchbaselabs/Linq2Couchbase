using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration
{
    public class JsonNetMemberNameResolver : IMemberNameResolver
    {

        public bool TryResolveMemberName(MemberInfo member, out string memberName)
        {
            memberName = null;

            if (member == null)
                return false;

            var contractResolver = ClusterHelper.Get().Configuration.SerializationSettings.ContractResolver;
            var contract = contractResolver.ResolveContract(member.DeclaringType);

            if (contract.GetType() == typeof(JsonObjectContract) && ((JsonObjectContract)contract).Properties.Any(p => p.UnderlyingName == member.Name && !p.Ignored))
            {
                memberName = ((JsonObjectContract)contract).Properties.First(p => p.UnderlyingName == member.Name).PropertyName;
                return true;
            }

            return false;
        }

    }
}
