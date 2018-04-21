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
    /// <typeparam name="TRow">Type of data row being proxied</typeparam>
    internal class DocumentProxyDataMapper<TRow> : IDataMapper
    {
        private readonly IExtendedTypeSerializer _serializer;
        private readonly IChangeTrackableContext _context;

        public DocumentProxyDataMapper(ITypeSerializer serializer, IChangeTrackableContext context)
        {
            _serializer = (serializer ?? throw new ArgumentNullException(nameof(serializer))) as IExtendedTypeSerializer;

            if (_serializer == null || !_serializer.SupportedDeserializationOptions.CustomObjectCreator)
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
        public T Map<T>(Stream stream) where T : class
        {
            var queryResults = _serializer.Deserialize<T>(stream);

            // The use of reflection here isn't terribly efficient.  However, for a N1QL query this method will
            // only be called once for a single IQueryResult<T>, so the performance penalty is very negligible.

            // Find a property returning IEnumerable<T>, this will be the rows of the result
            var property = queryResults.GetType().GetProperties()
                .FirstOrDefault(p => typeof(IEnumerable<TRow>).IsAssignableFrom(p.PropertyType));
            if (property != null)
            {
                // Map was called for an IQueryResult<T> object, so go through all of the rows and call
                // ITrackedDocumentNode.ClearStatus to indicate that deserialization is complete.

                ClearStatusOnQueryRequestRows(
                    (IEnumerable<TRow>) property.GetMethod.Invoke(queryResults, null),
                    _context);
            }

            return queryResults;
        }

        public static void ClearStatusOnQueryRequestRows(IEnumerable<TRow> rows, IChangeTrackableContext context)
        {
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var status = row as ITrackedDocumentNode;
                    if (status != null)
                    {
                        // Track the document
                        context.Track(row);

                        // Clear the deserialization flags to start change tracking
                        status.ClearStatus();
                    }
                }
            }
        }
    }
}
