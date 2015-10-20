using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Metadata;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.Clauses
{
    /// <summary>
    /// Tests related to the Nest clause
    /// </summary>
    [TestFixture]
    public class NestClauseTests
    {
        [Test]
        public void Nest_IEnumerableEmulation()
        {
            var outer = new[]
            {
                new Outer {Key = "outer1", Keys = new[] {"inner1", "inner2"}},
                new Outer {Key = "outer2", Keys = new[] {"inner3", "inner4", "inner6"}},
                new Outer {Key = "outer3", Keys = new string[] {}},
                new Outer {Key = "outer4", Keys = null}
            };

            var inner = new[] { "inner1", "inner2", "inner3", "inner4", "inner5" }
                .Select(key => new Inner { Key = key })
                .ToArray();

            var result = outer
                .Nest(
                    inner, 
                    p => p.Keys,
                    (outerDoc, innerDocs) => new Nested { OuterDoc = outerDoc, InnerDocs = innerDocs.OrderBy(p => p.GetMetadata().Id).ToArray()}
                )
                .OrderBy(p => p.OuterDoc.Key)
                .ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("outer1", result[0].OuterDoc.Key);
            Assert.AreEqual(2, result[0].InnerDocs.Length);
            Assert.AreEqual("inner1", result[0].InnerDocs[0].Key);
            Assert.AreEqual("inner2", result[0].InnerDocs[1].Key);
            Assert.AreEqual("outer2", result[1].OuterDoc.Key);
            Assert.AreEqual(2, result[1].InnerDocs.Length);
            Assert.AreEqual("inner3", result[1].InnerDocs[0].Key);
            Assert.AreEqual("inner4", result[1].InnerDocs[1].Key);
        }

        [Test]
        public void Nest_IEnumerableAsQueryable()
        {
            // This test is important for consumers who may use Nest clauses in their unit testing
            // If they are using .AsQueryable() to convert a simple list or array to IQueryable
            // Then we need to ensure that it is properly handled by the EnumerableQuery<T> LINQ implementation

            var outer = new[]
            {
                new Outer {Key = "outer1", Keys = new[] {"inner1", "inner2"}},
                new Outer {Key = "outer2", Keys = new[] {"inner3", "inner4", "inner6"}},
                new Outer {Key = "outer3", Keys = new string[] {}},
                new Outer {Key = "outer4", Keys = null}
            };

            var inner = new[] {"inner1", "inner2", "inner3", "inner4", "inner5"}
                .Select(key => new Inner {Key = key})
                .ToArray();

            var result = outer
                .AsQueryable()
                .Nest(
                    inner.AsQueryable(), 
                    p => p.Keys,
                    (outerDoc, innerDocs) => new Nested { OuterDoc = outerDoc, InnerDocs = innerDocs.OrderBy(p => p.GetMetadata().Id).ToArray() }
                )
                .OrderBy(p => p.OuterDoc.Key)
                .ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("outer1", result[0].OuterDoc.Key);
            Assert.AreEqual(2, result[0].InnerDocs.Length);
            Assert.AreEqual("inner1", result[0].InnerDocs[0].Key);
            Assert.AreEqual("inner2", result[0].InnerDocs[1].Key);
            Assert.AreEqual("outer2", result[1].OuterDoc.Key);
            Assert.AreEqual(2, result[1].InnerDocs.Length);
            Assert.AreEqual("inner3", result[1].InnerDocs[0].Key);
            Assert.AreEqual("inner4", result[1].InnerDocs[1].Key);
        }

        [Test]
        public void LeftOuterNest_IEnumerableEmulation()
        {
            var outer = new[]
            {
                new Outer {Key = "outer1", Keys = new[] {"inner1", "inner2"}},
                new Outer {Key = "outer2", Keys = new[] {"inner3", "inner4", "inner6"}},
                new Outer {Key = "outer3", Keys = new string[] {}},
                new Outer {Key = "outer4", Keys = null}
            };

            var inner = new[] { "inner1", "inner2", "inner3", "inner4", "inner5" }
                .Select(key => new Inner { Key = key })
                .ToArray();

            var result = outer
                .LeftOuterNest(
                    inner,
                    p => p.Keys,
                    (outerDoc, innerDocs) => new Nested { OuterDoc = outerDoc, InnerDocs = innerDocs != null ? innerDocs.OrderBy(p => p.GetMetadata().Id).ToArray() : null }
                )
                .OrderBy(p => p.OuterDoc.Key)
                .ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("outer1", result[0].OuterDoc.Key);
            Assert.AreEqual(2, result[0].InnerDocs.Length);
            Assert.AreEqual("inner1", result[0].InnerDocs[0].Key);
            Assert.AreEqual("inner2", result[0].InnerDocs[1].Key);
            Assert.AreEqual("outer2", result[1].OuterDoc.Key);
            Assert.AreEqual(2, result[1].InnerDocs.Length);
            Assert.AreEqual("inner3", result[1].InnerDocs[0].Key);
            Assert.AreEqual("inner4", result[1].InnerDocs[1].Key);
            Assert.AreEqual("outer3", result[2].OuterDoc.Key);
            Assert.AreEqual(0, result[2].InnerDocs.Length);
            Assert.AreEqual("outer4", result[3].OuterDoc.Key);
            Assert.IsNull(result[3].InnerDocs);
        }

        [Test]
        public void LeftOuterNest_IEnumerableAsQueryable()
        {
            // This test is important for consumers who may use Nest clauses in their unit testing
            // If they are using .AsQueryable() to convert a simple list or array to IQueryable
            // Then we need to ensure that it is properly handled by the EnumerableQuery<T> LINQ implementation

            var outer = new[]
            {
                new Outer {Key = "outer1", Keys = new[] {"inner1", "inner2"}},
                new Outer {Key = "outer2", Keys = new[] {"inner3", "inner4", "inner6"}},
                new Outer {Key = "outer3", Keys = new string[] {}},
                new Outer {Key = "outer4", Keys = null}
            };

            var inner = new[] { "inner1", "inner2", "inner3", "inner4", "inner5" }
                .Select(key => new Inner { Key = key })
                .ToArray();

            var result = outer
                .AsQueryable()
                .LeftOuterNest(
                    inner.AsQueryable(),
                    p => p.Keys,
                    (outerDoc, innerDocs) => new Nested { OuterDoc = outerDoc, InnerDocs = innerDocs != null ? innerDocs.OrderBy(p => p.GetMetadata().Id).ToArray() : null }
                )
                .OrderBy(p => p.OuterDoc.Key)
                .ToArray();

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("outer1", result[0].OuterDoc.Key);
            Assert.AreEqual(2, result[0].InnerDocs.Length);
            Assert.AreEqual("inner1", result[0].InnerDocs[0].Key);
            Assert.AreEqual("inner2", result[0].InnerDocs[1].Key);
            Assert.AreEqual("outer2", result[1].OuterDoc.Key);
            Assert.AreEqual(2, result[1].InnerDocs.Length);
            Assert.AreEqual("inner3", result[1].InnerDocs[0].Key);
            Assert.AreEqual("inner4", result[1].InnerDocs[1].Key);
            Assert.AreEqual("outer3", result[2].OuterDoc.Key);
            Assert.AreEqual(0, result[2].InnerDocs.Length);
            Assert.AreEqual("outer4", result[3].OuterDoc.Key);
            Assert.IsNull(result[3].InnerDocs);
        }

        #region Helper Classes

        private class Outer
        {
            public string Key { get; set; }
            public string[] Keys { get; set; }
        }

        private class Inner : IDocumentMetadataProvider
        {
            public string Key { get; set; }

            public DocumentMetadata GetMetadata()
            {
                return new DocumentMetadata() {Id = Key};
            }
        }

        private class Nested
        {
            public Outer OuterDoc { get; set; }
            public Inner[] InnerDocs { get; set; }
        }

        #endregion

    }
}
