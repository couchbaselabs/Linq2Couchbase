using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Couchbase.Linq.QueryGeneration
{

    /// <summary>
    /// Provides default method call translator provider for N1QL expressions.
    /// Uses classes implementing IMethodCallTranslator defined in Couchbase.Linq assembly
    /// </summary>
    public class DefaultMethodCallTranslatorProvider : IMethodCallTranslatorProvider
    {

        #region Static

        private static readonly Dictionary<MethodInfo, IMethodCallTranslator> Registry = CreateDefaultRegistry();

        private static Dictionary<MethodInfo, IMethodCallTranslator> CreateDefaultRegistry()
        {
            var query =
                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract && typeof (IMethodCallTranslator).IsAssignableFrom(type))
                    .SelectMany(type =>
                    {
                        var instance = (IMethodCallTranslator) Activator.CreateInstance(type);

                        return instance.SupportMethods.Select(method => new {instance, method});
                    });

            return query.ToDictionary(p => p.method, p => p.instance);
        } 

        #endregion

        public IMethodCallTranslator GetTranslator(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression == null)
            {
                throw new ArgumentNullException("methodCallExpression");
            }

            return GetItem(methodCallExpression.Method);
        }

        protected virtual IMethodCallTranslator GetItem(MethodInfo key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            IMethodCallTranslator translator;
            if (Registry.TryGetValue(key, out translator))
            {
                return translator;
            }

            if (key.IsGenericMethod && !key.IsGenericMethodDefinition)
            {
                if (Registry.TryGetValue(key.GetGenericMethodDefinition(), out translator))
                {
                    return translator;
                }
            }

            return null;
        }

    }
}
