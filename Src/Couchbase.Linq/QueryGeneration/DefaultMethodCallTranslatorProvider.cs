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
    internal class DefaultMethodCallTranslatorProvider : IMethodCallTranslatorProvider
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

            // Check if the generic form of the declaring type matches
            translator = GetItemFromGenericType(key);
            if (translator != null)
            {
                return translator;
            }

            // Check any interfaces that may have a matching method
            translator = GetItemFromInterfaces(key);
            if (translator != null)
            {
                return translator;
            }

            // Finally, check base method if this is a virtual method
            var baseMethod = key.GetBaseDefinition();
            if ((baseMethod != null) && (baseMethod != key))
            {
                return GetItem(baseMethod);
            }

            // No match found
            return null;
        }

        /// <summary>
        /// Checks the generic version of the type that implements the key method to see if it has an IMethodCallTranslator defined
        /// </summary>
        /// <param name="key">MethodInfo to test</param>
        /// <returns>Null if no IMethodCallTranslator is found</returns>
        protected virtual IMethodCallTranslator GetItemFromGenericType(MethodInfo key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if ((key.DeclaringType == null) ||
                (!key.DeclaringType.IsGenericType || key.DeclaringType.IsGenericTypeDefinition))
            {
                return null;
            }

            var genericType = key.DeclaringType.GetGenericTypeDefinition();

            var typeArgs = key.IsGenericMethod && !key.IsGenericMethodDefinition
                ? key.GetGenericArguments()
                : null;

            var bindingFlags = (key.IsStatic ? BindingFlags.Static : BindingFlags.Instance) |
                               (key.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic);

            var genericTypeKey = genericType.GetMethods(bindingFlags)
                .FirstOrDefault(method => method.Name == key.Name && ArgsMatch(key.GetParameters(), method, typeArgs));

            if (genericTypeKey != null)
            {
                IMethodCallTranslator translator;

                if (Registry.TryGetValue(genericTypeKey, out translator))
                {
                    return translator;
                }

                if (genericTypeKey.IsGenericMethod && !genericTypeKey.IsGenericMethodDefinition)
                {
                    if (Registry.TryGetValue(genericTypeKey.GetGenericMethodDefinition(), out translator))
                    {
                        return translator;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks all interfaces that implement the key method to see if one of them has an IMethodCallTranslator defined
        /// </summary>
        /// <param name="key">MethodInfo to test.  Must be an instance method on a class.</param>
        /// <returns>Null if no IMethodCallTranslator is found</returns>
        protected virtual IMethodCallTranslator GetItemFromInterfaces(MethodInfo key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (key.IsStatic || (key.DeclaringType == null) || (!key.DeclaringType.IsClass))
            {
                return null;
            }

            foreach (var interfaceType in key.DeclaringType.GetInterfaces())
            {
                var interfaceMap = key.DeclaringType.GetInterfaceMap(interfaceType);

                var interfaceMethod = interfaceMap.TargetMethods
                    .Where(p => p == key)
                    .Select((p, i) => interfaceMap.InterfaceMethods[i])
                    .FirstOrDefault();

                if (interfaceMethod != null)
                {
                    var translator = GetItem(interfaceMethod);
                    if (translator != null)
                    {
                        return translator;
                    }
                }
            }

            return null;
        }

        private bool ArgsMatch(ParameterInfo[] args, MethodInfo method, Type[] typeArgs)
        {
            if (!method.IsGenericMethodDefinition && (typeArgs != null))
            {
                return false;
            }

            if (method.IsGenericMethodDefinition)
            {
                if ((typeArgs == null) || (typeArgs.Length != method.GetGenericArguments().Length))
                {
                    return false;
                }

                method = method.MakeGenericMethod(typeArgs);
            }

            var methodArgs = method.GetParameters();
            if (methodArgs.Length != args.Length)
            {
                return false;
            }

            for (var i = 0; i < args.Length; i++)
            {
                if (!methodArgs[i].ParameterType.IsAssignableFrom(args[i].ParameterType))
                {
                    return false;
                }
            }

            return true;
        }

    }
}
