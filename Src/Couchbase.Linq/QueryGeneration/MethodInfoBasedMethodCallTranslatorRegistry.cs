using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Utilities;

namespace Couchbase.Linq.QueryGeneration
{
    public class MethodInfoBasedMethodCallTranslatorRegistry : RegistryBase<MethodInfoBasedMethodCallTranslatorRegistry, MethodInfo, IMethodCallTranslator>, IMethodCallTranslatorProvider
    {
        public IMethodCallTranslator GetTranslator(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression == null)
            {
                throw new ArgumentNullException("methodCallExpression");
            }

            var key = methodCallExpression.Method;

            return GetItem(key);
        }

        public override IMethodCallTranslator GetItem(MethodInfo key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            var transformer = GetItemExact(key);
            if (transformer != null)
                return transformer;

            if (key.IsGenericMethod && !key.IsGenericMethodDefinition)
                return GetItem(key.GetGenericMethodDefinition());

            var baseMethod = key.GetBaseDefinition();
            if (baseMethod != key)
                return GetItem(baseMethod);

            return null;
        }

        protected override void RegisterForTypes(IEnumerable<Type> itemTypes)
        {
            var supportedMethodsForTypes = from t in itemTypes
                                           let supportedMethodsField = t.GetField("SupportedMethods", BindingFlags.Static | BindingFlags.Public)
                                           where supportedMethodsField != null
                                           select new { Generator = t, Methods = (IEnumerable<MethodInfo>)supportedMethodsField.GetValue(null) };

            foreach (var supportedMethodsForType in supportedMethodsForTypes)
                Register(supportedMethodsForType.Methods, (IMethodCallTranslator)Activator.CreateInstance(supportedMethodsForType.Generator));
        }


    }
}
