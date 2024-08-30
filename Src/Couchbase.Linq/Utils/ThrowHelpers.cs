using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Couchbase.Linq.Utils
{
    internal static class ThrowHelpers
    {
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(argument, paramName);
#else
            if (argument is null)
            {
                ThrowArgumentNullException(paramName);
            }
#endif
        }

        [DoesNotReturn]
        public static void ThrowArgumentNullException(string? paramName) =>
            throw new ArgumentNullException(paramName);
    }
}
