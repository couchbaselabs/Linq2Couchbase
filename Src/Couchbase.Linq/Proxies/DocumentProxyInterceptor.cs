using System.Linq;
using Castle.DynamicProxy;
using Couchbase.Linq.Metadata;

namespace Couchbase.Linq.Proxies
{
    internal class DocumentProxyInterceptor : IInterceptor
    {
        private readonly DocumentNode _documentNode = new DocumentNode();

        /// <summary>
        /// Used by intercepted __metadata property generated on the proxy.  We use this special
        /// setter so that it isn't serialized back to the data store.
        /// </summary>
        /// <param name="metadata"></param>
        public void SetMetadata(DocumentMetadata metadata)
        {
            _documentNode.Metadata = metadata;
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.InvocationTarget == null)
            {
                // This is a call against an ITrackedDocumentNode member
                // So redirect to the members on the DocumentNode

                invocation.ReturnValue = invocation.Method.Invoke(_documentNode, invocation.Arguments);
                return;
            }

            // ReSharper disable once PossibleNullReferenceException
            var property = invocation.Method.DeclaringType.GetProperties()
                .FirstOrDefault(prop => prop.GetSetMethod() == invocation.Method);

            if (property == null)
            {
                // Not a property setter, don't intercept
                invocation.Proceed();
                return;
            }

            var getMethod = property.GetGetMethod();

            if (getMethod == null)
            {
                // Not a property with a getter and setter, don't intercept
                invocation.Proceed();
                return;
            }

            // Save the previous value
            object initialValue = getMethod.Invoke(invocation.InvocationTarget, null);

            invocation.Proceed();

            // Get the new value
            object finalValue = getMethod.Invoke(invocation.InvocationTarget, null);

            if ((initialValue == null) && (finalValue != null) ||
                (initialValue != null && !initialValue.Equals(finalValue)))
            {
                // Value was changed

                var status = initialValue as ITrackedDocumentNode;
                if (status != null)
                {
                    _documentNode.RemoveChild(status);
                }

                status = finalValue as ITrackedDocumentNode;
                if (status != null)
                {
                    _documentNode.AddChild(status);
                }

                _documentNode.DocumentModified(status);
            }
        }
    }
}
