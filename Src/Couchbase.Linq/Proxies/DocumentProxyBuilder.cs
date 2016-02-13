using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;

namespace Couchbase.Linq.Proxies
{
    /// <summary>
    /// Overrides <see cref="CreateClassProxyType"/> for an existing <see cref="IProxyBuilder"/> so that it uses
    /// <see cref="DocumentProxyGenerator"/> instead of the default <see cref="ClassProxyGenerator"/>.
    /// </summary>
    class DocumentProxyBuilder : IProxyBuilder
    {
        private readonly IProxyBuilder _baseProxyBuilder;

        public ILogger Logger
        {
            get { return _baseProxyBuilder.Logger; }
            set { _baseProxyBuilder.Logger = value; }
        }

        public ModuleScope ModuleScope
        {
            get { return _baseProxyBuilder.ModuleScope; }
        }

        /// <summary>
        /// Creates a DocumentProxyBuilder instance.
        /// </summary>
        public DocumentProxyBuilder() : this(new DefaultProxyBuilder())
        {
        }

        /// <summary>
        /// Creates a DocumentProxyBuilder instance.
        /// </summary>
        /// <param name="scope"><see cref="ModuleScope"/> to use for <see cref="DefaultProxyBuilder"/>.</param>
        public DocumentProxyBuilder(ModuleScope scope) : this(new DefaultProxyBuilder(scope))
        {
        }

        /// <summary>
        /// Creates a DocumentProxyBuilder instance.
        /// </summary>
        /// <param name="baseProxyBuilder">Base <see cref="IProxyBuilder"/> to use for all calls other than <see cref="CreateClassProxyType"/>.</param>
        public DocumentProxyBuilder(IProxyBuilder baseProxyBuilder)
        {
            if (baseProxyBuilder == null)
            {
                throw new ArgumentNullException("baseProxyBuilder");
            }

            _baseProxyBuilder = baseProxyBuilder;
        }

        public Type CreateClassProxyType(Type classToProxy, Type[] additionalInterfacesToProxy, ProxyGenerationOptions options)
        {
            var generator = new DocumentProxyGenerator(ModuleScope, classToProxy)
            {
                Logger = Logger
            };

            return generator.GenerateCode(additionalInterfacesToProxy, options);
        }

        public Type CreateClassProxyTypeWithTarget(Type classToProxy, Type[] additionalInterfacesToProxy,
            ProxyGenerationOptions options)
        {
            return _baseProxyBuilder.CreateClassProxyTypeWithTarget(classToProxy, additionalInterfacesToProxy, options);
        }

        public Type CreateInterfaceProxyTypeWithTarget(Type interfaceToProxy, Type[] additionalInterfacesToProxy, Type targetType,
            ProxyGenerationOptions options)
        {
            return _baseProxyBuilder.CreateInterfaceProxyTypeWithTarget(interfaceToProxy, additionalInterfacesToProxy, targetType, options);
        }

        public Type CreateInterfaceProxyTypeWithTargetInterface(Type interfaceToProxy, Type[] additionalInterfacesToProxy,
            ProxyGenerationOptions options)
        {
            return _baseProxyBuilder.CreateInterfaceProxyTypeWithTargetInterface(interfaceToProxy, additionalInterfacesToProxy, options);
        }

        public Type CreateInterfaceProxyTypeWithoutTarget(Type interfaceToProxy, Type[] additionalInterfacesToProxy,
            ProxyGenerationOptions options)
        {
            return _baseProxyBuilder.CreateInterfaceProxyTypeWithoutTarget(interfaceToProxy, additionalInterfacesToProxy, options);
        }
    }
}
