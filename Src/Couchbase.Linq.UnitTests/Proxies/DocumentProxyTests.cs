using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Configuration.Client;
using Couchbase.Linq.Proxies;
using Moq;
using NUnit.Framework;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Couchbase.Linq.UnitTests.Proxies
{
    [TestFixture]
    public class DocumentProxyTests
    {
        #region General

        [Test]
        public void ImplementsITrackedDocumentNode()
        {
            // Act

            var result = DocumentProxyManager.Default.CreateProxy(typeof (DocumentRoot));

            // Assert

            Assert.NotNull(result as ITrackedDocumentNode);
        }

        [Test]
        public void InterceptorsAreNotSerialized()
        {
            // Act

            var result = Newtonsoft.Json.JsonConvert.SerializeObject(DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot)));

            // Assert

            Assert.NotNull(result);
            Assert.False(result.Contains("__interceptors"));
        }

        [Test]
        public void MetadataIsNotSerialized()
        {
            // Arrange

            var configuration = new ClientConfiguration();
            var dataMapper = new DocumentProxyDataMapper<DocumentRoot>(configuration.Serializer.Invoke(), null);

            DocumentRoot proxy;
            using (var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes("{\"stringProperty\":\"value\",\"__metadata\":{\"id\":\"test\"}}")))
            {
                proxy = dataMapper.Map<DocumentRoot>(stream);
            }

            Assert.NotNull(proxy);
            Assert.NotNull(((ITrackedDocumentNode)proxy).Metadata);
            Assert.NotNull(((ITrackedDocumentNode)proxy).Metadata.Id);

            // Act

            var result = Encoding.UTF8.GetString(configuration.Serializer.Invoke().Serialize(proxy));

            // Assert

            Assert.NotNull(result);
            Assert.False(result.Contains("__metadata"));
            Assert.False(result.Contains("metadata"));
            Assert.False(result.Contains("Metadata"));
        }

        [Test]
        public void NoAction_IsNotDirty()
        {
            // Arrange

            var document = (DocumentRoot)DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot));

            // Act

            var result = ((ITrackedDocumentNode)document).IsDirty;

            // Assert

            Assert.IsFalse(result);
        }

        [Test]
        public void ClearStatus_IsNotDirty()
        {
            // Arrange

            var document = (DocumentRoot)DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot));

            document.StringProperty = "string";

            // Act

            ((ITrackedDocumentNode)document).ClearStatus();

            var result = ((ITrackedDocumentNode)document).IsDirty;

            // Assert

            Assert.IsFalse(result);
        }

        [Test]
        public void IsDeserializingAndMakeChanges_IsNotDirty()
        {
            // Arrange

            var document = (DocumentRoot)DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot));

            ((ITrackedDocumentNode) document).IsDeserializing = true;

            // Act

            document.StringProperty = "string";

            var result = ((ITrackedDocumentNode)document).IsDirty;

            // Assert

            Assert.IsFalse(result);
        }

        #endregion

        #region Simple Property

        [Test]
        public void SetPropertyFromNull_IsDirty()
        {
            // Arrange

            var document = (DocumentRoot) DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot));

            // Act

            document.StringProperty = "string";

            var result = ((ITrackedDocumentNode) document).IsDirty;

            // Assert

            Assert.IsTrue(result);
        }

        [Test]
        public void SetPropertyFromNullToNull_IsNotDirty()
        {
            // Arrange

            var document = (DocumentRoot)DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot));

            // Act

            document.StringProperty = null;

            var result = ((ITrackedDocumentNode)document).IsDirty;

            // Assert

            Assert.IsFalse(result);
        }

        [Test]
        public void SetPropertyToNull_IsDirty()
        {
            // Arrange

            var document = (DocumentRoot)DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot));

            document.StringProperty = "string";

            ((ITrackedDocumentNode)document).ClearStatus();

            // Act

            document.StringProperty = null;

            var result = ((ITrackedDocumentNode)document).IsDirty;

            // Assert

            Assert.IsTrue(result);
        }

        [Test]
        public void SetProperty_IsDirty()
        {
            // Arrange

            var document = (DocumentRoot)DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot));

            document.StringProperty = "string";

            ((ITrackedDocumentNode)document).ClearStatus();

            // Act

            document.StringProperty = "string2";

            var result = ((ITrackedDocumentNode)document).IsDirty;

            // Assert

            Assert.IsTrue(result);
        }

        #endregion

        #region SubDocument

        [Test]
        public void SetProperty_OnSubDocument_IsDirty()
        {
            // Arrange

            var document = (DocumentRoot)DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot));

            document.ObjectProperty = (SubDocument)DocumentProxyManager.Default.CreateProxy(typeof (SubDocument));

            ((ITrackedDocumentNode)document).ClearStatus();

            // Act

            document.ObjectProperty.IntegerProperty = 1;

            var result = ((ITrackedDocumentNode)document).IsDirty;

            // Assert

            Assert.IsTrue(result);
        }

        [Test]
        public void SetProperty_OnRemovedSubDocument_IsNotDirty()
        {
            // Arrange

            var originalSubDocument = (SubDocument)DocumentProxyManager.Default.CreateProxy(typeof(SubDocument));

            var document = (DocumentRoot)DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot));

            document.ObjectProperty = originalSubDocument;
            document.ObjectProperty = (SubDocument)DocumentProxyManager.Default.CreateProxy(typeof(SubDocument));

            ((ITrackedDocumentNode)document).ClearStatus();

            // Act

            originalSubDocument.IntegerProperty = 1;

            var result = ((ITrackedDocumentNode)document).IsDirty;

            // Assert

            Assert.IsFalse(result);
        }

        [Test]
        public void ClearStatus_SubDocument_IsNotDirty()
        {
            // Arrange

            var document = (DocumentRoot)DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot));

            document.ObjectProperty = (SubDocument)DocumentProxyManager.Default.CreateProxy(typeof(SubDocument));

            document.ObjectProperty.IntegerProperty = 1;

            // Act

            ((ITrackedDocumentNode)document).ClearStatus();

            var result = ((ITrackedDocumentNode)document.ObjectProperty).IsDirty;

            // Assert

            Assert.IsFalse(result);
        }

        [Test]
        public void ClearStatus_SubDocument_IsNotDeserializing()
        {
            // Arrange

            var document = (DocumentRoot)DocumentProxyManager.Default.CreateProxy(typeof(DocumentRoot));

            document.ObjectProperty = (SubDocument)DocumentProxyManager.Default.CreateProxy(typeof(SubDocument));

            ((ITrackedDocumentNode)document.ObjectProperty).IsDeserializing = true;

            // Act

            ((ITrackedDocumentNode)document).ClearStatus();

            var result = ((ITrackedDocumentNode)document.ObjectProperty).IsDeserializing;

            // Assert

            Assert.IsFalse(result);
        }

        #endregion

        #region Helpers

        public class DocumentRoot
        {
            public virtual string StringProperty { get; set; }

            public virtual SubDocument ObjectProperty { get; set; }
        }

        public class SubDocument
        {
            public virtual int IntegerProperty { get; set; }
        }

        #endregion
    }
}
