using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;
using Castle.DynamicProxy.Generators.Emitters;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Couchbase.Linq.Metadata;

namespace Couchbase.Linq.Proxies
{
    /// <summary>
    /// Inherited from <see cref="DocumentProxyGenerator"/>, this generator applies <see cref="IgnoreDataMemberAttribute"/>
    /// to the emitted "__interceptors" field.  This prevents it from  being serialized by JSON serializers.
    /// </summary>
    internal class DocumentProxyGenerator : ClassProxyGenerator
    {
        private const string InterceptorsFieldName = "__interceptors";

        private static readonly MethodInfo SetMetadata =
            typeof (DocumentProxyInterceptor).GetMethod("SetMetadata");

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

                CreateMetadataProperty(emitter, interceptorsField);
            }
        }

        /// <summary>
        /// Creates an intercepted property named __metadata, which only has a setter, which is used to deserialize
        /// the document metadata.  Since there is no getter, it won't be serialized back to the data store.
        /// </summary>
        private void CreateMetadataProperty(ClassEmitter emitter, FieldReference interceptorsField)
        {
            var propertyBuilder = emitter.TypeBuilder.DefineProperty("__metadata", PropertyAttributes.None, CallingConventions.HasThis,
                typeof (DocumentMetadata), null, null, null, null, null);

            var methodBuilder = emitter.TypeBuilder.DefineMethod("set___metadata", MethodAttributes.Public | MethodAttributes.SpecialName);
            methodBuilder.SetParameters(typeof (DocumentMetadata));

            var codeBuilder = methodBuilder.GetILGenerator();
            codeBuilder.Emit(OpCodes.Ldarg_0);
            codeBuilder.Emit(OpCodes.Ldfld, interceptorsField.Fieldbuilder);
            codeBuilder.Emit(OpCodes.Ldc_I4_0);
            codeBuilder.Emit(OpCodes.Ldelem, typeof(IInterceptor));
            codeBuilder.Emit(OpCodes.Castclass, typeof(DocumentProxyInterceptor));
            codeBuilder.Emit(OpCodes.Ldarg_1);
            codeBuilder.Emit(OpCodes.Call, SetMetadata);
            codeBuilder.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(methodBuilder);
        }
    }
}
