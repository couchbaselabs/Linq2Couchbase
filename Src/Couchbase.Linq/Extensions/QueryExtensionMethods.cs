using System.Linq;
using System.Reflection;
using System.Threading;

namespace Couchbase.Linq.Extensions
{
    /// <summary>
    /// Static helper to store reflection method info for various <see cref="IQueryable{T}"/> extensions.
    /// </summary>
    internal static class QueryExtensionMethods
    {
        public static MethodInfo FirstAsyncNoPredicate { get; }
        public static MethodInfo FirstAsyncWithPredicate { get; }
        public static MethodInfo FirstOrDefaultAsyncNoPredicate { get; }
        public static MethodInfo FirstOrDefaultAsyncWithPredicate { get; }

        public static MethodInfo SingleAsyncNoPredicate { get; }
        public static MethodInfo SingleAsyncWithPredicate { get; }
        public static MethodInfo SingleOrDefaultAsyncNoPredicate { get; }
        public static MethodInfo SingleOrDefaultAsyncWithPredicate { get; }

        public static MethodInfo AnyAsyncNoPredicate { get; }
        public static MethodInfo AnyAsyncWithPredicate { get; }
        public static MethodInfo AllAsync { get; }

        public static MethodInfo CountAsyncNoPredicate { get; }
        public static MethodInfo CountAsyncWithPredicate { get; }
        public static MethodInfo LongCountAsyncNoPredicate { get; }
        public static MethodInfo LongCountAsyncWithPredicate { get; }

        public static MethodInfo SumAsyncNoSelector { get; }
        public static MethodInfo SumAsyncWithSelector { get; }
        public static MethodInfo AverageAsyncNoSelector { get; }
        public static MethodInfo AverageAsyncWithSelector { get; }

        public static MethodInfo Nest { get; }
        public static MethodInfo LeftOuterNest { get; }
        public static MethodInfo Explain { get; }

        public static MethodInfo UseKeys { get; }
        public static MethodInfo UseIndexWithType { get; }
        public static MethodInfo UseHash { get; }

        public static MethodInfo ScanConsistency { get; }
        public static MethodInfo ConsistentWith { get; }

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

            SingleAsyncNoPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.SingleAsync) && p.GetParameters().Length == 1);
            SingleAsyncWithPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.SingleAsync) && p.GetParameters().Length == 2 && p.GetParameters().Last().ParameterType != typeof(CancellationToken));
            SingleOrDefaultAsyncNoPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.SingleOrDefaultAsync) && p.GetParameters().Length == 1);
            SingleOrDefaultAsyncWithPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.SingleOrDefaultAsync) && p.GetParameters().Length == 2 && p.GetParameters().Last().ParameterType != typeof(CancellationToken));

            AnyAsyncNoPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.AnyAsync) && p.GetParameters().Length == 1);
            AnyAsyncWithPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.AnyAsync) && p.GetParameters().Length == 2 && p.GetParameters().Last().ParameterType != typeof(CancellationToken));
            AllAsync = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.AllAsync) && p.GetParameters().Length == 2 && p.GetParameters().Last().ParameterType != typeof(CancellationToken));

            CountAsyncNoPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.CountAsync) && p.GetParameters().Length == 1);
            CountAsyncWithPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.CountAsync) && p.GetParameters().Length == 2 && p.GetParameters().Last().ParameterType != typeof(CancellationToken));
            LongCountAsyncNoPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.LongCountAsync) && p.GetParameters().Length == 1);
            LongCountAsyncWithPredicate = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.LongCountAsync) && p.GetParameters().Length == 2 && p.GetParameters().Last().ParameterType != typeof(CancellationToken));

            SumAsyncNoSelector = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.SumAsync) && p.GetParameters().Length == 1);
            SumAsyncWithSelector = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.SumAsync) && p.GetParameters().Length == 2 && p.GetParameters().Last().ParameterType != typeof(CancellationToken));
            AverageAsyncNoSelector = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.AverageAsync) && p.GetParameters().Length == 1);
            AverageAsyncWithSelector = allMethods.Single(p =>
                p.Name == nameof(QueryExtensions.AverageAsync) && p.GetParameters().Length == 2 && p.GetParameters().Last().ParameterType != typeof(CancellationToken));

            Nest = allMethods.Single(p => p.Name == nameof(QueryExtensions.Nest));
            LeftOuterNest = allMethods.Single(p => p.Name == nameof(QueryExtensions.LeftOuterNest));
            Explain = allMethods.Single(p => p.Name == nameof(QueryExtensions.Explain));

            UseKeys = allMethods.Single(p => p.Name == nameof(QueryExtensions.UseKeys));
            UseIndexWithType = allMethods.Single(p => p.Name == nameof(QueryExtensions.UseIndex) && p.GetParameters().Length == 3);
            UseHash = allMethods.Single(p => p.Name == nameof(QueryExtensions.UseHash));

            ScanConsistency = allMethods.Single(p => p.Name == nameof(QueryExtensions.ScanConsistency));
            ConsistentWith = allMethods.Single(p => p.Name == nameof(QueryExtensions.ConsistentWith));
        }
    }
}
