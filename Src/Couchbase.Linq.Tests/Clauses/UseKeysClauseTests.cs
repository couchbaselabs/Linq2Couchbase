using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Metadata;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.Tests.Clauses
{
    /// <summary>
    /// Tests related to the UseKeys clause
    /// </summary>
    [TestFixture]
    public class UseKeysClauseTests
    {
        [Test]
        public void UseKeys_IEnumerableEmulation()
        {
            var items = new[]
            {
                new Item {Key = "item1"},
                new Item {Key = "item2"},
                new Item {Key = "item3"},
                new Item {Key = "item4"}
            };

            var result = items
                .UseKeys(new[] { "item1", "item3"})
                .OrderBy(p => p.Key)
                .ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("item1", result[0].Key);
            Assert.AreEqual("item3", result[1].Key);
        }

        [Test]
        public void UseKeys_IEnumerableAsQueryable()
        {
            // This test is important for consumers who may use UseKeys clauses in their unit testing
            // If they are using .AsQueryable() to convert a simple list or array to IQueryable
            // Then we need to ensure that it is properly handled by the EnumerableQuery<T> LINQ implementation

            var items = new[]
            {
                new Item {Key = "item1"},
                new Item {Key = "item2"},
                new Item {Key = "item3"},
                new Item {Key = "item4"}
            };

            var result = items
                .AsQueryable()
                .UseKeys(new[] { "item1", "item3" })
                .OrderBy(p => p.Key)
                .ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("item1", result[0].Key);
            Assert.AreEqual("item3", result[1].Key);
        }

        #region Helper Classes

        private class Item : IDocumentMetadataProvider
        {
            public string Key { get; set; }

            public DocumentMetadata GetMetadata()
            {
                return new DocumentMetadata() { Id = Key };
            }
        }

        #endregion

    }
}
