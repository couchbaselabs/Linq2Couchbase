using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Couchbase.Linq.QueryGeneration.MemberNameResolvers
{
    internal interface IMemberNameResolver
    {
        bool TryResolveMemberName(MemberInfo member, [MaybeNullWhen(false)] out string memberName);
    }
}