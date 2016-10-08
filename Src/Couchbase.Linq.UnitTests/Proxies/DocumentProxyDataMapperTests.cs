using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Configuration.Client;
using Couchbase.Core.Serialization;
using Couchbase.Linq.Proxies;
using Couchbase.N1QL;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.Proxies
{
    [TestFixture]
    public class DocumentProxyDataMapperTests
    {
        [Test]
        public void New_NotIExtendedTypeSerializer_ThrowsNotSupportedException()
        {
            // Arrange

            var configuration = new ClientConfiguration()
            {
                Serializer = () => new Mock<ITypeSerializer>().Object
            };

            // Act/Assert

            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<NotSupportedException>(() => new DocumentProxyDataMapper<Document>(configuration, new Mock<IChangeTrackableContext>().Object));
        }

        [Test]
        public void New_CustomObjectCreatorNotSupported_ThrowsNotSupportedException()
        {
            // Arrange

            var serializer = new Mock<IExtendedTypeSerializer>();
            serializer.SetupGet(p => p.SupportedDeserializationOptions).Returns(new SupportedDeserializationOptions()
            {
                CustomObjectCreator = false
            });

            var configuration = new ClientConfiguration()
            {
                Serializer = () => serializer.Object
            };

            // Act/Assert

            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<NotSupportedException>(() => new DocumentProxyDataMapper<Document>(configuration, new Mock<IChangeTrackableContext>().Object));
        }

        [Test]
        public void Map_QueryResponse_RowsAreNotDirty()
        {
            // Arrange

            var configuration = new ClientConfiguration()
            {
                Serializer = () => new FakeSerializer()
            };

            var dataMapper = new DocumentProxyDataMapper<Document>(configuration, new Mock<IChangeTrackableContext>().Object);

            // Act

            var result = dataMapper.Map<QueryResult<Document>>(null);

            // Assert

            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.True(result.Rows.All(p => !((ITrackedDocumentNode) p).IsDirty));
        }

        [Test]
        public void Map_QueryResponse_RowsAreNotDeserializing()
        {
            // Arrange

            var configuration = new ClientConfiguration()
            {
                Serializer = () => new FakeSerializer()
            };

            var dataMapper = new DocumentProxyDataMapper<Document>(configuration, new Mock<IChangeTrackableContext>().Object);

            // Act

            var result = dataMapper.Map<QueryResult<Document>>(null);

            // Assert

            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.True(result.Rows.All(p => !((ITrackedDocumentNode)p).IsDeserializing));
        }

        #region Helpers

        private class FakeSerializer : IExtendedTypeSerializer
        {
            public SupportedDeserializationOptions SupportedDeserializationOptions
            {
                get {
                    return new SupportedDeserializationOptions()
                    {
                        CustomObjectCreator = true
                    };
                }
            }

            public DeserializationOptions DeserializationOptions { get; set; }

            public T Deserialize<T>(byte[] buffer, int offset, int length)
            {
                throw new NotImplementedException();
            }

            public T Deserialize<T>(Stream stream)
            {
                var result = new QueryResult<Document>();
                result.Rows.Add((Document) DocumentProxyManager.Default.CreateProxy(typeof (Document)));
                result.Rows.Add((Document)DocumentProxyManager.Default.CreateProxy(typeof(Document)));

                // make the rows dirty
                result.Rows[0].IntegerProperty = 1;
                result.Rows[1].IntegerProperty = 2;

                // make the rows deserialiaing
                // ReSharper disable SuspiciousTypeConversion.Global
                ((ITrackedDocumentNode) result.Rows[0]).IsDeserializing = true;
                ((ITrackedDocumentNode) result.Rows[1]).IsDeserializing = true;
                // ReSharper restore SuspiciousTypeConversion.Global

                return (T) (object) result;
            }

            public byte[] Serialize(object obj)
            {
                throw new NotImplementedException();
            }

            public string GetMemberName(MemberInfo member)
            {
                throw new NotImplementedException();
            }
        }

        public class Document
        {
            public int IntegerProperty { get; set; }
        }

        #endregion
    }
}
