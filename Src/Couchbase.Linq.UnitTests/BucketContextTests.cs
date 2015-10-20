using Couchbase.Core;
using Couchbase.IO;
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
            var route = new Route();
            var bucket = new Mock<IBucket>();
            var ctx = new BucketContext(bucket.Object);
            Assert.Throws<KeyAttributeMissingException>(()=>ctx.GetDocumentId(route));
        }

        [Test]
        public void GetDocumentId_When_DocId_Exists_Use_It()
        {
            var beer = new Beer {Name = "beer1"};
            var bucket = new Mock<IBucket>();
            var ctx = new BucketContext(bucket.Object);
            var id = ctx.GetDocumentId(beer);
            Assert.AreEqual("beer1", id);
        }

       [Test]
        public void Save_When_Write_Is_Succesful_Return_Success()
        {
            var beer = new Beer();
            var bucket = new Mock<IBucket>();
            var result = new Mock<IOperationResult<Beer>> ();
            result.Setup(x => x.Status).Returns(ResponseStatus.Success);
            result.Setup(x => x.Success).Returns(true);
            bucket.Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<Beer>())).Returns(result.Object);
            var ctx = new BucketContext(bucket.Object);
            ctx.Save(beer);
        }

       [Test]
       public void Save_When_Write_Is_Not_Succesful_Throw_CouchbaseWriteException()
       {
           var beer = new Beer();
           var bucket = new Mock<IBucket>();
           var result = new Mock<IOperationResult<Beer>>();
           result.Setup(x => x.Success).Returns(false);
           bucket.Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<Beer>())).Returns(result.Object);
           var ctx = new BucketContext(bucket.Object);
           Assert.Throws<CouchbaseWriteException>(()=>ctx.Save(beer));
       }

       [Test]
       public void Remove_When_Write_Is_Not_Succesful_Throw_CouchbaseWriteException()
       {
           var beer = new Beer();
           var bucket = new Mock<IBucket>();
           var result = new Mock<IOperationResult<Beer>>();
           result.Setup(x => x.Success).Returns(false);
           bucket.Setup(x => x.Remove(It.IsAny<string>())).Returns(result.Object);
           var ctx = new BucketContext(bucket.Object);
           Assert.Throws<CouchbaseWriteException>(() => ctx.Remove(beer));
       }

       [Test]
       public void Save_When_KeyAttribute_Is_Not_Defined_Throw_DocumentIdMissingException()
       {
           var brewery = new Brewery();
           var bucket = new Mock<IBucket>();
           var result = new Mock<IOperationResult<Brewery>>();
           result.Setup(x => x.Status).Returns(ResponseStatus.Success);
           bucket.Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<Brewery>())).Returns(result.Object);
           var ctx = new BucketContext(bucket.Object);
           Assert.Throws<KeyAttributeMissingException>(()=>ctx.Save(brewery));
       }

       [Test]
       public void Remove_When_Write_Is_Succesful_Return_Success()
       {
           var beer = new Beer();
           var bucket = new Mock<IBucket>();
           var result = new Mock<IOperationResult<Beer>>();
           result.Setup(x => x.Status).Returns(ResponseStatus.Success);
           result.Setup(x => x.Success).Returns(true);
           bucket.Setup(x => x.Remove(It.IsAny<string>())).Returns(result.Object);
           var ctx = new BucketContext(bucket.Object);
           ctx.Remove(beer);
       }

       [Test]
       public void Remove_When_DocId_Is_Not_Defined_Throw_DocumentIdMissingException()
       {
           var brewery = new Brewery();
           var bucket = new Mock<IBucket>();
           var result = new Mock<IOperationResult<Brewery>>();
           result.Setup(x => x.Status).Returns(ResponseStatus.Success);
           bucket.Setup(x => x.Upsert(It.IsAny<string>(), It.IsAny<Brewery>())).Returns(result.Object);
           var ctx = new BucketContext(bucket.Object);
           Assert.Throws<KeyAttributeMissingException>(() => ctx.Remove(brewery));
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
