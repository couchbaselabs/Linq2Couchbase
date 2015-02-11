using System.Reflection;

namespace Couchbase.Linq.QueryGeneration
{
    public interface IMemberNameResolver
    {
        bool TryResolveMemberName(MemberInfo member, out string memberName);
    }
}