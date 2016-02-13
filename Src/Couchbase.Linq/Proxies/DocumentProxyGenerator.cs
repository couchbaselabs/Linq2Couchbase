using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;
using Castle.DynamicProxy.Generators.Emitters;

namespace Couchbase.Linq.Proxies
{
    /// <summary>
    /// Inherited from <see cref="DocumentProxyGenerator"/>, this generator applies <see cref="IgnoreDataMemberAttribute"/>
    /// to the emitted "__interceptors" field.  This prevents it from  being serialized by JSON serializers.
    /// </summary>
    internal class DocumentProxyGenerator : ClassProxyGenerator
    {
        private const string InterceptorsFieldName = "__interceptors";

        public DocumentProxyGenerator(ModuleScope scope, Type targetType) : base(scope, targetType)
        {
        }

        protected override void CreateFields(ClassEmitter emitter)
        {
            base.CreateFields(emitter);

            var interceptorsField = emitter.GetField(InterceptorsFieldName);
            if (interceptorsField != null)
            {
                emitter.DefineCustomAttributeFor<IgnoreDataMemberAttribute>(interceptorsField);
            }
        }
    }
}
