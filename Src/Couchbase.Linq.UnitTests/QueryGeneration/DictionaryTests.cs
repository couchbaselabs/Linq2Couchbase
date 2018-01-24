using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class DictionaryTests : N1QLTestBase
    {
        #region Index

        [Test]
        public void Test_Index_Interface()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<DictionaryInterface>(mockBucket.Object)
                .Select(p => p.Dictionary["key"]);

            const string expected = "SELECT `Extent1`.`Dictionary`.`key` as `result` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Index_Class()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<DictionaryClass>(mockBucket.Object)
                .Select(p => p.Dictionary["key"]);

            const string expected = "SELECT `Extent1`.`Dictionary`.`key` as `result` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Index_InterfaceUntyped()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<DictionaryInterfaceUntyped>(mockBucket.Object)
                .Select(p => p.Dictionary["key"]);

            const string expected = "SELECT `Extent1`.`Dictionary`.`key` as `result` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Index

        [Test]
        public void Test_ContainsKey_Interface()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<DictionaryInterface>(mockBucket.Object)
                .Where(p => p.Dictionary.ContainsKey("key"));

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE `Extent1`.`Dictionary`.`key` IS NOT MISSING";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ContainsKey_Class()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<DictionaryClass>(mockBucket.Object)
                .Where(p => p.Dictionary.ContainsKey("key"));

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE `Extent1`.`Dictionary`.`key` IS NOT MISSING";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ContainsKey_InterfaceUntyped()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<DictionaryInterfaceUntyped>(mockBucket.Object)
                .Where(p => p.Dictionary.Contains("key"));

            const string expected = "SELECT `Extent1`.* FROM `default` as `Extent1` WHERE `Extent1`.`Dictionary`.`key` IS NOT MISSING";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable CollectionNeverUpdated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class DictionaryInterface
        {
            public IDictionary<string, Beer> Dictionary { get; set; }
        }

        private class DictionaryClass
        {
            public Dictionary<string, Beer> Dictionary { get; set; }
        }

        private class DictionaryInterfaceUntyped
        {
            public IDictionary Dictionary { get; set; }
        }


        // ReSharper restore ClassNeverInstantiated.Local
        // ReSharper restore CollectionNeverUpdated.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
}
