using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Couchbase.Linq.Proxies
{
    /// <summary>
    /// Manages the creation of proxies for document nodes.  These proxies implement <see cref="ITrackedDocumentNode"/> for change tracking.
    /// Proxies will track changes on any property implemented as virtual.  Any such property which is assigned an
    /// object which also implements <see cref="ITrackedDocumentNode"/> will have changes tracked if the child
    /// node changes.
    /// </summary>
    /// <remarks>
    /// For performance reasons, should be used as a singleton.  This then makes <see cref="Castle.DynamicProxy.ProxyGenerator"/> a singleton as well,
    /// which means it will cache and reuse the proxy types.  The <see cref="DocumentProxyManager.Default"/> property
    /// provides a singleton instance with a default <see cref="Castle.DynamicProxy.ProxyGenerator"/> for normal use.
    /// </remarks>
    internal class DocumentProxyManager
    {
        private static readonly Type[] InterfacesToProxy = {typeof (ITrackedDocumentNode)};

        private static readonly ProxyGenerationOptions Options = new ProxyGenerationOptions()
        {
            Hook = new DocumentProxyGenerationHook()
        };

        /// <summary>
        /// Singleton instance of <see cref="DocumentProxyManager"/> for use creating document proxies.
        /// </summary>
        public static DocumentProxyManager Default { get; set; } = new DocumentProxyManager();

        private readonly ProxyGenerator _proxyGenerator;
        /// <summary>
        /// <see cref="Castle.DynamicProxy.ProxyGenerator"/> used to create proxies.
        /// </summary>
        public ProxyGenerator ProxyGenerator
        {
            get { return _proxyGenerator; }
        }

        /// <summary>
        /// Creates a new DocumentProxyManager with a default <see cref="Castle.DynamicProxy.ProxyGenerator"/>.
        /// </summary>
        public DocumentProxyManager() : this(new ProxyGenerator())
        {
        }

        /// <summary>
        /// Creates a new DocumentProxyManager with a given <see cref="Castle.DynamicProxy.ProxyGenerator"/>.
        /// </summary>
        /// <param name="proxyGenerator"><see cref="Castle.DynamicProxy.ProxyGenerator"/> used to create proxies.</param>
        public DocumentProxyManager(ProxyGenerator proxyGenerator)
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException("proxyGenerator");
            }

            _proxyGenerator = proxyGenerator;
        }

        /// <summary>
        /// Create a proxy of a document that implements <see cref="ITrackedDocumentNode"/> for change tracking.
        /// Proxy will track changes on any property implemented as virtual.  Any such property which is assigned an
        /// object which also implements <see cref="ITrackedDocumentNode"/> will have changes tracked if the child
        /// node changes.
        /// </summary>
        /// <param name="documentType">Type of document to proxy.</param>
        /// <returns>New instance of a proxy implementing <see cref="ITrackedDocumentNode"/>.</returns>
        public virtual object CreateProxy(Type documentType)
        {
            return ProxyGenerator.CreateClassProxy(documentType, InterfacesToProxy, Options,
                    new DocumentProxyInterceptor());
        }
    }
}
