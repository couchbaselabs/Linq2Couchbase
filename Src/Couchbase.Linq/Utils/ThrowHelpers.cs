using System;
using System.Diagnostics.CodeAnalysis;

namespace Couchbase.Linq.Utils
{
    internal static class ThrowHelpers
    {
        [DoesNotReturn]
        public static void ThrowArgumentNullException(string paramName) =>
            throw new ArgumentNullException(paramName);
    }
}
