using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration
{
    public interface IMemberNameResolver
    {
        string ResolveMemberName(MemberInfo member);
    }
}
