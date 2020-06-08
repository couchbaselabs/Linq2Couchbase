using System;

namespace Couchbase.Linq.Utils
{
    internal static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider) =>
            (T) serviceProvider.GetService(typeof(T));

        public static T GetRequiredService<T>(this IServiceProvider serviceProvider) =>
            serviceProvider.GetService<T>() ??
            throw new InvalidOperationException($"Service {typeof(T).FullName} not registered.");
    }
}
