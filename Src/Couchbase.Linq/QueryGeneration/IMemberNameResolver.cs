using System.Reflection;

namespace Couchbase.Linq.QueryGeneration
{
    internal interface IMemberNameResolver
    {
        bool TryResolveMemberName(MemberInfo member, out string memberName);
    }
}