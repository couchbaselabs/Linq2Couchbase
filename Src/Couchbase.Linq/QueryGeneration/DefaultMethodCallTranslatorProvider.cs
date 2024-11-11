using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Couchbase.Linq.QueryGeneration.MethodCallTranslators;

namespace Couchbase.Linq.QueryGeneration
{

    /// <summary>
    /// Provides default method call translator provider for N1QL expressions.
    /// Uses classes implementing <see cref="IMethodCallTranslator" /> defined in Couchbase.Linq assembly,
    /// as well as any method which implements <see cref="N1QlFunctionAttribute" />.
    /// </summary>
    internal class DefaultMethodCallTranslatorProvider : IMethodCallTranslatorProvider
    {

        #region Static

        private static readonly Dictionary<MethodInfo, IMethodCallTranslator> Registry = CreateDefaultRegistry();

        private static Dictionary<MethodInfo, IMethodCallTranslator> CreateDefaultRegistry()
        {
            var query = typeof(DefaultMethodCallTranslatorProvider).GetTypeInfo().Assembly
                .GetTypes()
                .Where(
                    type =>
                        type.GetTypeInfo().IsClass && !type.GetTypeInfo().IsAbstract &&
                        typeof (IMethodCallTranslator).IsAssignableFrom(type) &&
                        type.GetConstructor(Type.EmptyTypes) != null)
                .SelectMany(type =>
                {
                    var instance = (IMethodCallTranslator) Activator.CreateInstance(type)!;

                    return instance.SupportMethods
                        .Where(method => method != null)
                        .Select(method => new {instance, method});
                });

            return query.ToDictionary(p => p.method, p => p.instance);
        }

        #endregion

        public IMethodCallTranslator? GetTranslator(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression == null)
            {
                throw new ArgumentNullException(nameof(methodCallExpression));
            }

            return GetItem(methodCallExpression.Method);
        }

        protected virtual IMethodCallTranslator? GetItem(MethodInfo key)
        {
            lock (Registry)
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (Registry.TryGetValue(key, out var translator))
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

                // If no preregistered method is found, check to see if the method is decorated with N1QlFunctionAttribute
                translator = CreateFromN1QlFunctionAttribute(key);
                if (translator != null)
                {
                    // Save this translator for reuse
                    Registry.Add(key, translator);

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
        }

        /// <summary>
        /// Checks the generic version of the type that implements the key method to see if it has an IMethodCallTranslator defined
        /// </summary>
        /// <param name="key">MethodInfo to test</param>
        /// <returns>Null if no IMethodCallTranslator is found</returns>
        protected virtual IMethodCallTranslator? GetItemFromGenericType(MethodInfo key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if ((key.DeclaringType == null) ||
                (!key.DeclaringType.GetTypeInfo().IsGenericType || key.DeclaringType.GetTypeInfo().IsGenericTypeDefinition))
            {
                return null;
            }

            var genericType = key.DeclaringType.GetGenericTypeDefinition();
            var classTypeArgs = key.DeclaringType.GetGenericArguments();

            var methodTypeArgs = key.IsGenericMethod && !key.IsGenericMethodDefinition
                ? key.GetGenericArguments()
                : null;

            var bindingFlags = (key.IsStatic ? BindingFlags.Static : BindingFlags.Instance) |
                               (key.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic);

            var genericTypeKey = genericType.GetMethods(bindingFlags)
                .FirstOrDefault(method =>
                    method.Name == key.Name && ArgsMatch(key.GetParameters(), method, classTypeArgs, methodTypeArgs));

            if (genericTypeKey != null)
            {
                lock (Registry)
                {
                    if (Registry.TryGetValue(genericTypeKey, out var translator))
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
            }

            return null;
        }

        /// <summary>
        /// Checks all interfaces that implement the key method to see if one of them has an IMethodCallTranslator defined
        /// </summary>
        /// <param name="key">MethodInfo to test.  Must be an instance method on a class.</param>
        /// <returns>Null if no IMethodCallTranslator is found</returns>
        protected virtual IMethodCallTranslator? GetItemFromInterfaces(MethodInfo key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.IsStatic || (key.DeclaringType == null) || (!key.DeclaringType.GetTypeInfo().IsClass))
            {
                return null;
            }

            foreach (var interfaceType in key.DeclaringType.GetInterfaces())
            {
                var interfaceMap = key.DeclaringType.GetTypeInfo().GetRuntimeInterfaceMap(interfaceType);

                var matchedMapping = interfaceMap.TargetMethods
                    .Select((p, i) => new {ClassMethod = p, InterfaceMethod = interfaceMap.InterfaceMethods[i]})
                    .FirstOrDefault(p => p.ClassMethod == key);

                if (matchedMapping != null)
                {
                    var translator = GetItem(matchedMapping.InterfaceMethod);
                    if (translator != null)
                    {
                        return translator;
                    }
                }
            }

            return null;
        }

        private static bool ArgsMatch(ParameterInfo[] args, MethodInfo method, Type[] classTypeArgs, Type[]? methodTypeArgs)
        {
            if (!method.IsGenericMethodDefinition && (methodTypeArgs != null))
            {
                return false;
            }

            if (method.IsGenericMethodDefinition)
            {
                if ((methodTypeArgs == null) || (methodTypeArgs.Length != method.GetGenericArguments().Length))
                {
                    return false;
                }

                method = method.MakeGenericMethod(methodTypeArgs);
            }

            var methodArgs = method.GetParameters();
            if (methodArgs.Length != args.Length)
            {
                return false;
            }

            for (var i = 0; i < args.Length; i++)
            {
                var parameterType = methodArgs[i].ParameterType;
                if (parameterType.IsGenericParameter)
                {
                    // This parameter is a generic from the class (not a method generic), so convert before comparing
                    var genericArgs = method.DeclaringType!.GetGenericArguments();

                    for (var genericIndex = 0; genericIndex < genericArgs.Length; genericIndex++)
                    {
                        if (genericArgs[genericIndex] == parameterType)
                        {
                            parameterType = classTypeArgs[genericIndex];
                            break;
                        }
                    }
                }

                if (!parameterType.IsAssignableFrom(args[i].ParameterType))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks for <see cref="N1QlFunctionAttribute" /> and creates a new <see cref="N1QlFunctionMethodCallTranslator" />
        /// if it is found.  If not found, returns null.
        /// </summary>
        protected virtual IMethodCallTranslator? CreateFromN1QlFunctionAttribute(MethodInfo key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var attribute = key.GetCustomAttribute<N1QlFunctionAttribute>(true);

            return attribute == null
                ? null
                : new N1QlFunctionMethodCallTranslator(key, attribute);
        }
    }
}
