using System.Linq;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.IO;
using Couchbase.Linq.Proxies;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests
{
    [TestFixture]
    public class BucketContextTests
    {
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
        public void Save_When_Write_Is_Succesful_Return_Success()
        {
            //arrange
            var beer = new Beer();
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
            var beer = new Beer();
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
            var beer = new Beer();
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
            var beer = new Beer();
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
