using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq
{
    internal static class TypeSystem
    {

        internal static Type GetElementType(Type seqType)
        {
            var iEnumerable = FindIEnumerable(seqType);
            return iEnumerable == null ? seqType : iEnumerable.GetGenericArguments()[0];
        }

        private static Type FindIEnumerable(Type seqType)
        {
            while (true)
            {
                if (seqType == null || seqType == typeof (string))
                {
                    return null;
                }
                if (seqType.IsArray)
                {
                    return typeof (IEnumerable<>).MakeGenericType(seqType.GetElementType());
                }
                if (seqType.IsGenericType)
                {
                    foreach (var arg in seqType.GetGenericArguments())
                    {
                        var iEnumerable = typeof (IEnumerable<>).MakeGenericType(arg);
                        if (iEnumerable.IsAssignableFrom(seqType))
                        {
                            return iEnumerable;
                        }
                    }
                }

                var interfaces = seqType.GetInterfaces();
                if (interfaces.Length > 0)
                {
                    foreach (var iface in interfaces)
                    {
                        var iEnumerable = FindIEnumerable(iface);
                        if (iEnumerable != null) return iEnumerable;
                    }
                }
                if (seqType.BaseType != null && seqType.BaseType != typeof (object))
                {
                    seqType = seqType.BaseType;
                    continue;
                }
                return null;
            }
        }
    }
}
