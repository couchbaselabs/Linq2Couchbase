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
        public string ResolveMemberName(MemberInfo member)
        {
            if (member == null)
                return null;

            var memberName = member.Name;

            var contractResolver = ClusterHelper.Get().Configuration.SerializationSettings.ContractResolver;
            var contract = contractResolver.ResolveContract(member.DeclaringType);

            if (contract.GetType() == typeof(JsonObjectContract) && ((JsonObjectContract)contract).Properties.Any(p => p.UnderlyingName == member.Name))
            {
                memberName = ((JsonObjectContract)contract).Properties.First(p => p.UnderlyingName == member.Name).PropertyName;
            }

            return memberName;
        }
    }
}
