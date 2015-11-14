using System.Reflection;

namespace Couchbase.Linq.QueryGeneration.MemberNameResolvers
{
    internal interface IMemberNameResolver
    {
        bool TryResolveMemberName(MemberInfo member, out string memberName);
    }
}