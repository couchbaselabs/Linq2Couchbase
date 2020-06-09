using System.Linq;
using System.Reflection;
using System.Threading;

namespace Couchbase.Linq.Extensions
{
    /// <summary>
    /// Static helper to store reflection method info for various <see cref="IQueryable"/> extensions.
    /// </summary>
    internal static class QueryExtensionMethods
    {
        public static MethodInfo FirstAsyncNoPredicate { get; }
        public static MethodInfo FirstAsyncWithPredicate { get; }
        public static MethodInfo FirstOrDefaultAsyncNoPredicate { get; }
        public static MethodInfo FirstOrDefaultAsyncWithPredicate { get; }

        static QueryExtensionMethods()
        {
            var allMethods = typeof(QueryExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .ToList();

            FirstAsyncNoPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.FirstAsync) && p.GetParameters().Length == 1);
            FirstAsyncWithPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.FirstAsync) && p.GetParameters().Length == 2 && p.GetParameters().Last().ParameterType != typeof(CancellationToken));
            FirstOrDefaultAsyncNoPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.FirstOrDefaultAsync) && p.GetParameters().Length == 1);
            FirstOrDefaultAsyncWithPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.FirstOrDefaultAsync) && p.GetParameters().Length == 2 && p.GetParameters().Last().ParameterType != typeof(CancellationToken));
        }
    }
}
