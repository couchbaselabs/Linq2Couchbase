using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Couchbase.Linq.Extensions
{
    /// <summary>
    /// Static helper to store reflection method info for various <see cref="IEnumerable{T}"/> extensions.
    /// </summary>
    internal static class EnumerableExtensionMethods
    {
        public static MethodInfo Nest { get; }
        public static MethodInfo LeftOuterNest { get; }

        public static MethodInfo UseKeys { get; }

        static EnumerableExtensionMethods()
        {
            var allMethods = typeof(EnumerableExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .ToList();

            Nest = allMethods.Single(p => p.Name == nameof(QueryExtensions.Nest));
            LeftOuterNest = allMethods.Single(p => p.Name == nameof(QueryExtensions.LeftOuterNest));

            UseKeys = allMethods.Single(p => p.Name == nameof(QueryExtensions.UseKeys));
        }
    }
}
