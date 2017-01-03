using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Core.Buckets;
using Couchbase.IO;
using Couchbase.Linq.Metadata;
using Couchbase.Linq.Proxies;
using Couchbase.Linq.UnitTests.Documents;
using Couchbase.Linq.Utils;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests
{
    [TestFixture]
    public class BucketContextTests
    {
        [Test]
        public void Can_Get_The_Bucket_The_Context_Was_Created_With()
        {
            var bucket = new Mock<IBucket>();
            var context = new BucketContext(bucket.Object);

            Assert.AreSame(bucket.Object, context.Bucket);
        }

        [Test]
        public void GetDocumentId_When_Id_Field_DoesNotExist_Throw_KeyAttributeMissingException()
        {
            //arrange
            var route = new Route();
            var bucket = new Mock<IBucket>();
            var ctx = new BucketContext(bucket.Object);

            //act-assert
            Assert.Throws<KeyAttributeMissingException>(() => ctx.GetDocumentId(route));
        }

        [Test]
        public void GetDocumentId_When_DocId_Exists_Use_It()
        {
            //arrange
            var beer = new Beer {Name = "beer1"};
            var bucket = new Mock<IBucket>();
            var ctx = new BucketContext(bucket.Object);

            //act
            var id = ctx.GetDocumentId(beer);

            //assert
            Assert.AreEqual("beer1", id);
        }

        [Test]
        public void GetDocumentId_NonString_ConvertsToString()
        {
            //arrange
            var document = new IntegerIdDocument { Id = 2 };
            var bucket = new Mock<IBucket>();
            var ctx = new BucketContext(bucket.Object);

            //act
            var id = ctx.GetDocumentId(document);

            //assert
            Assert.AreEqual("2", id);
        }

        [Test]
        public void GetDocumentId_NullKey_ThrowsKeyNullException()
        {
            //arrange
            var document = new Beer { Name = null };
            var bucket = new Mock<IBucket>();
            var ctx = new BucketContext(bucket.Object);

            //act
            var ex = Assert.Throws<KeyNullException>(() => ctx.GetDocumentId(document));

            //assert
            Assert.AreEqual(ExceptionMsgs.KeyNull, ex.Message);
        }

        [Test]
        public void GetDocumentId_DocumentProxyVirtualA_UsesMetadataNotProperty()
        {
            //arrange
            var document = (VirtualKeyA) DocumentProxyManager.Default.CreateProxy(typeof(VirtualKeyA));
            document.A = "Key1";

            // ReSharper disable once SuspiciousTypeConversion.Global
            var trackedDoc = (ITrackedDocumentNode) document;
            trackedDoc.Metadata = new DocumentMetadata()
            {
                Id = "Key2"
            };

            var bucket = new Mock<IBucket>();
            var ctx = new BucketContext(bucket.Object);

            //act
            var result = ctx.GetDocumentId(document);

            //assert
            Assert.AreEqual("Key2", result);
        }

        [Test]
        public void GetDocumentId_DocumentProxyNonVirtualA_UsesMetadataNotProperty()
        {
            //arrange
            var document = (NonVirtualKeyA)DocumentProxyManager.Default.CreateProxy(typeof(NonVirtualKeyA));
            document.A = "Key1";

            // ReSharper disable once SuspiciousTypeConversion.Global
            var trackedDoc = (ITrackedDocumentNode)document;
            trackedDoc.Metadata = new DocumentMetadata()
            {
                Id = "Key2"
            };

            var bucket = new Mock<IBucket>();
            var ctx = new BucketContext(bucket.Object);

            //act
            var result = ctx.GetDocumentId(document);

            //assert
            Assert.AreEqual("Key2", result);
        }

        [Test]
        public void GetDocumentId_DocumentProxyVirtualZ_UsesMetadataNotProperty()
        {
            //arrange
            var document = (VirtualKeyZ)DocumentProxyManager.Default.CreateProxy(typeof(VirtualKeyZ));
            document.Z = "Key1";

            // ReSharper disable once SuspiciousTypeConversion.Global
            var trackedDoc = (ITrackedDocumentNode)document;
            trackedDoc.Metadata = new DocumentMetadata()
            {
                Id = "Key2"
            };

            var bucket = new Mock<IBucket>();
            var ctx = new BucketContext(bucket.Object);

            //act
            var result = ctx.GetDocumentId(document);

            //assert
            Assert.AreEqual("Key2", result);
        }

        [Test]
        public void GetDocumentId_DocumentProxyNonVirtualZ_UsesMetadataNotProperty()
        {
            //arrange
            var document = (NonVirtualKeyZ)DocumentProxyManager.Default.CreateProxy(typeof(NonVirtualKeyZ));
            document.Z = "Key1";

            // ReSharper disable once SuspiciousTypeConversion.Global
            var trackedDoc = (ITrackedDocumentNode)document;
            trackedDoc.Metadata = new DocumentMetadata()
            {
                Id = "Key2"
            };

            var bucket = new Mock<IBucket>();
            var ctx = new BucketContext(bucket.Object);

            //act
            var result = ctx.GetDocumentId(document);

            //assert
            Assert.AreEqual("Key2", result);
        }

        [Test]
        public void Save_When_Write_Is_Succesful_Return_Success()
        {
            //arrange
            var beer = new Beer { Name = "beer1" };
            var bucket = new Mock<IBucket>();
            var result = new Mock<IOperationResult<Beer>>();
            result.Setup(x => x.Status).Returns(ResponseStatus.Success);
            result.Setup(x => x.Success).Returns(true);
            bucket.Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<Beer>())).Returns(result.Object);
            var ctx = new BucketContext(bucket.Object);

            //act
            ctx.Save(beer);

            //assert - does not throw exception
        }

        [Test]
        public void Save_When_Write_Is_Not_Succesful_Throw_CouchbaseWriteException()
        {
            //arrange
            var beer = new Beer { Name = "beer1" };
            var bucket = new Mock<IBucket>();
            var result = new Mock<IOperationResult<Beer>>();
            result.Setup(x => x.Success).Returns(false);
            bucket.Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<Beer>())).Returns(result.Object);
            var ctx = new BucketContext(bucket.Object);

            //act
            Assert.Throws<CouchbaseWriteException>(() => ctx.Save(beer));
        }

        [Test]
        public void Remove_When_Write_Is_Not_Succesful_Throw_CouchbaseWriteException()
        {
            //arrange
            var beer = new Beer { Name = "beer1"};
            var bucket = new Mock<IBucket>();
            var result = new Mock<IOperationResult<Beer>>();
            result.Setup(x => x.Success).Returns(false);
            bucket.Setup(x => x.Remove(It.IsAny<string>())).Returns(result.Object);
            var ctx = new BucketContext(bucket.Object);

            //act-assert
            Assert.Throws<CouchbaseWriteException>(() => ctx.Remove(beer));
        }

        [Test]
        public void Save_When_KeyAttribute_Is_Not_Defined_Throw_DocumentIdMissingException()
        {
            //arrange
            var brewery = new Brewery();
            var bucket = new Mock<IBucket>();
            var result = new Mock<IOperationResult<Brewery>>();
            result.Setup(x => x.Status).Returns(ResponseStatus.Success);
            bucket.Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<Brewery>())).Returns(result.Object);
            var ctx = new BucketContext(bucket.Object);

            //act-assert
            Assert.Throws<KeyAttributeMissingException>(() => ctx.Save(brewery));
        }

        [Test]
        public void Remove_When_Write_Is_Succesful_Return_Success()
        {
            //arrange
            var beer = new Beer { Name = "beer1" };
            var bucket = new Mock<IBucket>();
            var result = new Mock<IOperationResult<Beer>>();
            result.Setup(x => x.Status).Returns(ResponseStatus.Success);
            result.Setup(x => x.Success).Returns(true);
            bucket.Setup(x => x.Remove(It.IsAny<string>())).Returns(result.Object);
            var ctx = new BucketContext(bucket.Object);

            //act-assert
            ctx.Remove(beer);
        }

        [Test]
        public void Remove_When_DocId_Is_Not_Defined_Throw_DocumentIdMissingException()
        {
            //arrange
            var brewery = new Brewery();
            var bucket = new Mock<IBucket>();
            var ctx = new BucketContext(bucket.Object);

            //act-assert
            Assert.Throws<KeyAttributeMissingException>(() => ctx.Remove(brewery));
        }

        [Test]
        public void BeginChangeTracking_ChangeTrackingEnabled_Is_True()
        {
            //arrange
            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);
            var ctx = new BucketContext(bucket.Object);

            //act
            ctx.BeginChangeTracking();

            //assert
            Assert.IsTrue(ctx.ChangeTrackingEnabled);
        }

        [Test]
        public void EndChangeTracking_ChangeTrackingEnabled_Is_False()
        {
            //arrange
            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            //act
            ctx.EndChangeTracking();

            //assert
            Assert.IsFalse(ctx.ChangeTrackingEnabled);
        }

        [Test]
        public void BeginChangeTracking_CalledTwiceThenEndChangeTracking_ChangTrackingEnabledIsTrue()
        {
            //arrange
            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);

            var ctx = new BucketContext(bucket.Object);

            //act
            ctx.BeginChangeTracking();
            ctx.BeginChangeTracking();
            ctx.EndChangeTracking();

            //assert
            Assert.IsTrue(ctx.ChangeTrackingEnabled);
        }

        [Test]
        public void BeginChangeTracking_CalledTwiceThenEndChangeTrackingTwice_ChangTrackingEnabledIsFalse()
        {
            //arrange
            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);

            var ctx = new BucketContext(bucket.Object);

            //act
            ctx.BeginChangeTracking();
            ctx.BeginChangeTracking();
            ctx.EndChangeTracking();
            ctx.EndChangeTracking();

            //assert
            Assert.IsFalse(ctx.ChangeTrackingEnabled);
        }


        [Test]
        public void Save_Adds_New_Document_To_Modified_List()
        {
            //arrange
            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var beer = new Beer
            {
                Name = "doc1" //key field
            };

            //act
            ctx.Save(beer);

            //assert
            Assert.AreEqual(1, ctx.ModifiedCount);
        }

        [Test]
        public void Save_Updates_Duplicate_Document_In_Modified_List()
        {
            //arrange
            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var beer = new Beer
            {
                Name = "doc1" //key field
            };

            //act
            ctx.Save(beer);
            ctx.Save(beer);

            //assert
            Assert.AreEqual(1, ctx.ModifiedCount);
        }

        [Test]
        public void SubmitChanges_Removes_Document_From_Modified_List()
        {
            //arrange
            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var beer = new Beer
            {
                Name = "doc1" //key field
            };

            ctx.Save(beer);

            //act
            ctx.SubmitChanges();

            //assert
            Assert.AreEqual(0, ctx.ModifiedCount);
        }

        [Test]
        public void SubmitChanges_WhenCalled_DoesNotClearTrackedList()
        {
            //arrange
            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var beer = new Beer
            {
                Name = "doc1" //key field
            };

            ctx.Save(beer);

            //act
            ctx.SubmitChanges();

            //assert
            Assert.AreEqual(1, ctx.TrackedCount);
        }

        [Test]
        public void SubmitChanges_RemovedDoc_NoConsistencyCheck_DoesntPassCas()
        {
            //arrange

            var fakeResult = new Mock<IOperationResult>();
            fakeResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            fakeResult.Setup(m => m.Success).Returns(true);

            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);
            bucket
                .Setup(x => x.Remove(It.IsAny<string>()))
                .Returns(fakeResult.Object);
            bucket
                .Setup(x => x.Remove(It.IsAny<string>(), It.IsAny<ulong>()))
                .Returns(fakeResult.Object);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var metadata = new DocumentMetadata()
            {
                Cas = 100,
                Id = "Test"
            };

            var beer = new Mock<Beer>();
            beer.SetupAllProperties();

            var trackedDoc = beer.As<ITrackedDocumentNode>();
            trackedDoc.Setup(m => m.Metadata).Returns(metadata);
            trackedDoc.Setup(m => m.IsDeleted).Returns(true);

            ((IChangeTrackableContext) ctx).Track(beer.Object);
            ((IChangeTrackableContext) ctx).Modified(beer.Object);

            //act
            ctx.SubmitChanges(new SaveOptions()
            {
                PerformConsistencyCheck = false
            });

            //assert
            bucket.Verify(x => x.Remove(It.IsAny<string>()), Times.Once);
            bucket.Verify(x => x.Remove(It.IsAny<string>(), It.IsAny<ulong>()), Times.Never);
        }

        [Test]
        public void SubmitChanges_RemovedDoc_WithConsistencyCheck_PassesCas()
        {
            //arrange

            var fakeResult = new Mock<IOperationResult>();
            fakeResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            fakeResult.Setup(m => m.Success).Returns(true);

            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);
            bucket
                .Setup(x => x.Remove(It.IsAny<string>()))
                .Returns(fakeResult.Object);
            bucket
                .Setup(x => x.Remove(It.IsAny<string>(), It.IsAny<ulong>()))
                .Returns(fakeResult.Object);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var metadata = new DocumentMetadata()
            {
                Cas = 100,
                Id = "Test"
            };

            var beer = new Mock<Beer>();
            beer.SetupAllProperties();

            var trackedDoc = beer.As<ITrackedDocumentNode>();
            trackedDoc.Setup(m => m.Metadata).Returns(metadata);
            trackedDoc.Setup(m => m.IsDeleted).Returns(true);

            ((IChangeTrackableContext)ctx).Track(beer.Object);
            ((IChangeTrackableContext)ctx).Modified(beer.Object);

            //act
            ctx.SubmitChanges(new SaveOptions());

            //assert
            bucket.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
            bucket.Verify(x => x.Remove(It.IsAny<string>(), (ulong) trackedDoc.Object.Metadata.Cas), Times.Once);
        }

        [Test]
        public void SubmitChanges_ModifiedDoc_NoConsistencyCheck_DoesntPassCas()
        {
            //arrange

            var fakeResult = new Mock<IOperationResult<object>>();
            fakeResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            fakeResult.Setup(m => m.Success).Returns(true);

            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);
            bucket
                .Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(fakeResult.Object);
            bucket
                .Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<ulong>()))
                .Returns(fakeResult.Object);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var metadata = new DocumentMetadata()
            {
                Cas = 100,
                Id = "Test"
            };

            var beer = new Mock<Beer>();
            beer.SetupAllProperties();

            var trackedDoc = beer.As<ITrackedDocumentNode>();
            trackedDoc.Setup(m => m.Metadata).Returns(metadata);
            trackedDoc.Setup(m => m.IsDeleted).Returns(false);
            trackedDoc.Setup(m => m.IsDirty).Returns(true);

            ((IChangeTrackableContext)ctx).Track(beer.Object);
            ((IChangeTrackableContext)ctx).Modified(beer.Object);

            //act
            ctx.SubmitChanges(new SaveOptions()
            {
                PerformConsistencyCheck = false
            });

            //assert
            bucket.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            bucket.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<ulong>()), Times.Never);
        }

        [Test]
        public void SubmitChanges_ModifiedDoc_WithConsistencyCheck_PassesCas()
        {
            //arrange

            var fakeResult = new Mock<IOperationResult<object>>();
            fakeResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            fakeResult.Setup(m => m.Success).Returns(true);

            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);
            bucket
                .Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(fakeResult.Object);
            bucket
                .Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<ulong>()))
                .Returns(fakeResult.Object);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var metadata = new DocumentMetadata()
            {
                Cas = 100,
                Id = "Test"
            };

            var beer = new Mock<Beer>();
            beer.SetupAllProperties();

            var trackedDoc = beer.As<ITrackedDocumentNode>();
            trackedDoc.Setup(m => m.Metadata).Returns(metadata);
            trackedDoc.Setup(m => m.IsDeleted).Returns(false);
            trackedDoc.Setup(m => m.IsDirty).Returns(true);

            ((IChangeTrackableContext)ctx).Track(beer.Object);
            ((IChangeTrackableContext)ctx).Modified(beer.Object);

            //act
            ctx.SubmitChanges(new SaveOptions());

            //assert
            bucket.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            bucket.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>(), (ulong) trackedDoc.Object.Metadata.Cas), Times.Once);
        }

        [Test]
        public void SubmitChanges_NewDoc_NoConsistencyCheck_UsesUpsert()
        {
            //arrange

            var fakeResult = new Mock<IOperationResult<object>>();
            fakeResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            fakeResult.Setup(m => m.Success).Returns(true);

            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);
            bucket
                .Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(fakeResult.Object);
            bucket
                .Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(fakeResult.Object);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var beer = new Beer()
            {
                Name = "Test"
            };

            ctx.Save(beer);

            //act
            ctx.SubmitChanges(new SaveOptions()
            {
                PerformConsistencyCheck = false
            });

            //assert
            bucket.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            bucket.Verify(x => x.Insert(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void SubmitChanges_NewDoc_WithConsistencyCheck_UsesInsert()
        {
            //arrange

            var fakeResult = new Mock<IOperationResult<object>>();
            fakeResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            fakeResult.Setup(m => m.Success).Returns(true);

            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);
            bucket
                .Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(fakeResult.Object);
            bucket
                .Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(fakeResult.Object);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var beer = new Beer()
            {
                Name = "Test"
            };

            ctx.Save(beer);

            //act
            ctx.SubmitChanges(new SaveOptions());

            //assert
            bucket.Verify(x => x.Upsert(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            bucket.Verify(x => x.Insert(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Test]
        public void EndChangeTracking_WhenCalled_ClearsTrackedList()
        {
            //arrange
            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var beer = new Beer
            {
                Name = "doc1" //key field
            };

            ctx.Save(beer);

            //act
            ctx.EndChangeTracking();

            //assert
            Assert.AreEqual(0, ctx.TrackedCount);
        }

        [Test]
        public void Save_WhenChangeTrackingEnabled_AddsToTrackedList()
        {
            //arrange
            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var beer = new Beer
            {
                Name = "doc1" //key field
            };

            ctx.Save(beer);

            Assert.AreEqual(1, ctx.TrackedCount);
        }

        [Test]
        public void GetDocumentId_WhenChangeTrackingEnabled_ProxyUses__idFieldForKey()
        {
            var document = (Beer)DocumentProxyManager.Default.CreateProxy(typeof(Beer));

            ((ITrackedDocumentNode) document).Metadata.Id = "thekey";

            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var key = ctx.GetDocumentId(document);

            Assert.AreEqual("thekey", key);
        }

        [Test]
        public void EndChangeTracking_WhenCalled_ModifiedList()
        {
            //arrange
            var bucket = new Mock<IBucket>();
            bucket.Setup(x => x.Configuration).Returns(new ClientConfiguration().BucketConfigs.First().Value);

            var ctx = new BucketContext(bucket.Object);
            ctx.BeginChangeTracking();

            var beer = new Beer
            {
                Name = "doc1" //key field
            };

            ctx.Save(beer);

            //act
            ctx.EndChangeTracking();

            //assert
            Assert.AreEqual(0, ctx.ModifiedCount);
        }

        #region Mutation State Updates

        [Test]
        public void Save_NotTrackingChanges_AddsToMutationState()
        {
            // Arrange

            var token = new MutationToken("default", 1, 2, 3);

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");
            bucket
                .Setup(m => m.Upsert(It.IsAny<string>(), It.IsAny<Beer>()))
                .Returns(() =>
                {
                    var result = new Mock<IOperationResult<Beer>>();
                    result.SetupGet(p => p.Success).Returns(true);
                    result.SetupGet(p => p.Token).Returns(token);

                    return result.Object;
                });

            var db = new Mock<BucketContext>(bucket.Object)
            {
                CallBase = true
            };
            db.Setup(m => m.GetDocumentId(It.IsAny<Beer>())).Returns("id");

            // Act

            db.Object.Save(new Beer());

            // Assert

            db.Verify(m => m.AddToMutationState(token), Times.Once);
        }

        [Test]
        public void Remove_NotTrackingChanges_AddsToMutationState()
        {
            // Arrange

            var token = new MutationToken("default", 1, 2, 3);

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");
            bucket
                .Setup(m => m.Remove(It.IsAny<string>()))
                .Returns(() =>
                {
                    var result = new Mock<IOperationResult<Beer>>();
                    result.SetupGet(p => p.Success).Returns(true);
                    result.SetupGet(p => p.Token).Returns(token);

                    return result.Object;
                });

            var db = new Mock<BucketContext>(bucket.Object)
            {
                CallBase = true
            };
            db.Setup(m => m.GetDocumentId(It.IsAny<Beer>())).Returns("id");

            // Act

            db.Object.Remove(new Beer());

            // Assert

            db.Verify(m => m.AddToMutationState(token), Times.Once);
        }

        [Test]
        public void SubmitChanges_WithSave_AddsToMutationState()
        {
            // Arrange

            var token = new MutationToken("default", 1, 2, 3);

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");
            bucket
                .Setup(m => m.Insert(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(() =>
                {
                    var result = new Mock<IOperationResult<Beer>>();
                    result.SetupGet(p => p.Success).Returns(true);
                    result.SetupGet(p => p.Token).Returns(token);

                    return result.Object;
                });

            var db = new Mock<BucketContext>(bucket.Object)
            {
                CallBase = true
            };
            db.Setup(m => m.GetDocumentId(It.IsAny<Beer>())).Returns("id");

            db.Object.BeginChangeTracking();
            db.Object.Save(new Beer());

            // Act

            db.Object.SubmitChanges();

            // Assert

            db.Verify(m => m.AddToMutationState(token), Times.Once);
        }

        [Test]
        public void SubmitChanges_WithRemove_AddsToMutationState()
        {
            // Arrange

            var token = new MutationToken("default", 1, 2, 3);

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");
            bucket
                .Setup(m => m.Remove(It.IsAny<string>()))
                .Returns(() =>
                {
                    var result = new Mock<IOperationResult<Beer>>();
                    result.SetupGet(p => p.Success).Returns(true);
                    result.SetupGet(p => p.Token).Returns(token);

                    return result.Object;
                });

            var db = new Mock<BucketContext>(bucket.Object)
            {
                CallBase = true
            };
            db.Setup(m => m.GetDocumentId(It.IsAny<object>())).Returns("id");

            var document = new Mock<ITrackedDocumentNode>();
            document.SetupGet(m => m.Metadata).Returns(new DocumentMetadata()
            {
                Id = "id"
            });
            document.SetupAllProperties();

            db.Object.BeginChangeTracking();
            ((IChangeTrackableContext) db.Object).Track(document.Object);
            db.Object.Remove(document.Object);

            // Act

            db.Object.SubmitChanges();

            // Assert

            db.Verify(m => m.AddToMutationState(token), Times.Once);
        }

        #endregion

        #region AddToMutationState

        [Test]
        public void AddToMutationState_NullToken_DoesNothing()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");

            var db = new BucketContext(bucket.Object);

            // Act

            db.AddToMutationState(null);

            // Assert

            Assert.IsNull(db.MutationState);
        }

        [Test]
        public void AddToMutationState_DefaultToken_DoesNothing()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");

            var db = new BucketContext(bucket.Object);

            // Act

            db.AddToMutationState(new MutationToken("default", -1, -1, 1));

            // Assert

            Assert.IsNull(db.MutationState);
        }

        [Test]
        public void AddToMutationState_FirstRealToken_CreatesMutationState()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");

            var db = new BucketContext(bucket.Object);

            var token = new MutationToken("default", 1, 2, 3);

            // Act

            db.AddToMutationState(token);

            // Assert

            Assert.IsNotNull(db.MutationState);

            var tokens = MutationStateToList(db.MutationState);
            Assert.AreEqual(1, tokens.Count);
            Assert.Contains(token, tokens);
        }

        [Test]
        public void AddToMutationState_FirstRealTokenThenNull_DoesNothing()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");

            var db = new BucketContext(bucket.Object);

            var token = new MutationToken("default", 1, 2, 3);
            db.AddToMutationState(token);

            // Act

            db.AddToMutationState(null);

            // Assert

            Assert.IsNotNull(db.MutationState);

            var tokens = MutationStateToList(db.MutationState);
            Assert.AreEqual(1, tokens.Count);
            Assert.Contains(token, tokens);
        }

        [Test]
        public void AddToMutationState_FirstRealTokenThenDefaultToken_DoesNothing()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");

            var db = new BucketContext(bucket.Object);

            var token = new MutationToken("default", 1, 2, 3);
            db.AddToMutationState(token);

            // Act

            db.AddToMutationState(new MutationToken("default", -1, -1, -1));

            // Assert

            Assert.IsNotNull(db.MutationState);

            var tokens = MutationStateToList(db.MutationState);
            Assert.AreEqual(1, tokens.Count);
            Assert.Contains(token, tokens);
        }

        [Test]
        public void AddToMutationState_TwoRealTokens_CombinesTokens()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");

            var db = new BucketContext(bucket.Object);

            var token1 = new MutationToken("default", 1, 2, 3);
            var token2 = new MutationToken("default", 4, 5, 6);

            // Act

            db.AddToMutationState(token1);
            db.AddToMutationState(token2);

            // Assert

            Assert.IsNotNull(db.MutationState);

            var tokens = MutationStateToList(db.MutationState);
            Assert.AreEqual(2, tokens.Count);
            Assert.Contains(token1, tokens);
            Assert.Contains(token2, tokens);
        }

        private static List<MutationToken> MutationStateToList(N1QL.MutationState state)
        {
            var result = new List<MutationToken>();
            using (var enumerator = state.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }
            }

            return result;
        }

        #endregion

        #region ResetMutationState

        [Test]
        public void ResetMutationState_NoState_StillNull()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");

            var db = new BucketContext(bucket.Object);
            Assert.Null(db.MutationState);

            // Act

            db.ResetMutationState();

            // Assert

            Assert.Null(db.MutationState);
        }

        [Test]
        public void ResetMutationState_HasState_SetsToNull()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.SetupGet(m => m.Name).Returns("default");

            var db = new BucketContext(bucket.Object);

            db.AddToMutationState(new MutationToken("default", 1, 2, 3));
            Assert.NotNull(db.MutationState);

            // Act

            db.ResetMutationState();

            // Assert

            Assert.Null(db.MutationState);
        }

        #endregion

        #region Helpers

        private class IntegerIdDocument
        {
            [Key]
            public int Id { get; set; }
        }

        public class VirtualKeyA
        {
            [Key]
            public virtual string A { get; set; }
        }

        public class NonVirtualKeyA
        {
            [Key]
            public string A { get; set; }
        }

        public class VirtualKeyZ
        {
            [Key]
            public virtual string Z { get; set; }
        }

        public class NonVirtualKeyZ
        {
            [Key]
            public string Z { get; set; }
        }

        #endregion
    }
}

#region [ License information          ]

/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2015 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/

#endregion
