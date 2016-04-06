using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Configuration.Client;
using Couchbase.Core.Serialization;
using Couchbase.N1QL;
using Couchbase.Views;
using Newtonsoft.Json;

namespace Couchbase.Linq.Proxies
{
    /// <summary>
    /// Alternate IDataMapper to use for reading N1QL queries which generates document proxies that implement change tracking.
    /// Requires that <see cref="ClientConfiguration.Serializer"/> be an instance of <see cref="IExtendedTypeSerializer"/>.  The serializer
    /// must also support <see cref="SupportedDeserializationOptions.CustomObjectCreator"/>.
    /// </summary>
    internal class DocumentProxyDataMapper : IDataMapper
    {
        private readonly IExtendedTypeSerializer _serializer;
        private readonly IChangeTrackableContext _context;

        public DocumentProxyDataMapper(ClientConfiguration configuration, IChangeTrackableContext context)
        {

            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            _serializer = configuration.Serializer.Invoke() as IExtendedTypeSerializer;
            if (_serializer == null)
            {
                throw new NotSupportedException("Change tracking is not supported without an IExtendedTypeSerializer which supports CustomObjectCreator.");
            }

            if (!_serializer.SupportedDeserializationOptions.CustomObjectCreator)
            {
                throw new NotSupportedException("Change tracking is not supported without an IExtendedTypeSerializer which supports CustomObjectCreator.");
            }

            _context = context;

            _serializer.DeserializationOptions = new DeserializationOptions
            {
                CustomObjectCreator = new DocumentProxyTypeCreator()
            };
        }

        /// <summary>
        /// Maps a single row.
        /// </summary>
        /// <typeparam name="T">The type of document to deserialize.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> results of the query.</param>
        /// <returns>An object deserialized to it's T type.</returns>
        public T Map<T>(Stream stream)
        {
            var queryResults = _serializer.Deserialize<T>(stream);

            // The use of reflection here isn't terribly efficient.  However, for a N1QL query this method will
            // only be called once for a single IQueryResult<T>, so the performance penalty is very negligible.

            var queryResultInterface = typeof (T).GetInterfaces()
                .FirstOrDefault(p => p.IsGenericType && (p.GetGenericTypeDefinition() == typeof (IQueryResult<>)));
            if (queryResultInterface != null)
            {
                // Map was called for an IQueryResult<T> object, so go through all of the rows and call
                // ITrackedDocumentNode.ClearStatus to indicate that deserialization is complete.

                var methodInfo =
                    ClearStatusOnQueryRequestRowsMethodInfo.MakeGenericMethod(
                        queryResultInterface.GenericTypeArguments[0]);

                methodInfo.Invoke(this, new object[] {queryResults, _context});
            }

            return queryResults;
        }

        private static readonly MethodInfo ClearStatusOnQueryRequestRowsMethodInfo =
            typeof (DocumentProxyDataMapper).GetMethod("ClearStatusOnQueryRequestRows");

        public static void ClearStatusOnQueryRequestRows<T>(IQueryResult<T> result, IChangeTrackableContext context)
        {
            if (result.Rows != null)
            {
                foreach (var row in result.Rows)
                {
                    var status = row as ITrackedDocumentNode;
                    if (status != null)
                    {
                        // Track the document
                        context.Track(row);

                        // Register the context so that it can handled the changed document
                        status.RegisterChangeTracking(context);

                        // Clear the deserialization flags to start change tracking
                        status.ClearStatus();
                    }
                }
            }
        }
    }
}
