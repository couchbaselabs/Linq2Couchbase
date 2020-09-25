using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    class EnumTests : N1QLTestBase
    {
        #region No enum converter

        [Test]
        public void Test_NoEnumConverter_WhereEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<IntegerDoc>(mockBucket.Object)
                .Where(p => p.Value == IntegerEnum.Value0);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 0)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NoEnumConverter_WhereEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<IntegerDoc>(mockBucket.Object)
                .Where(p => IntegerEnum.Value0 == p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 0)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NoEnumConverter_WhereNotEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<IntegerDoc>(mockBucket.Object)
                .Where(p => p.Value != IntegerEnum.Value1);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 1)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NoEnumConverter_WhereNotEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<IntegerDoc>(mockBucket.Object)
                .Where(p => IntegerEnum.Value1 != p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 1)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region No enum converter on nullable value

        [Test]
        public void Test_NoEnumConverterOnNullable_WhereEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<IntegerNullableDoc>(mockBucket.Object)
                .Where(p => p.Value == IntegerEnum.Value0);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 0)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NoEnumConverterOnNullable_WhereEqualToNull()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<IntegerNullableDoc>(mockBucket.Object)
                .Where(p => p.Value == null);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` IS NULL)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NoEnumConverterOnNullable_WhereEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<IntegerNullableDoc>(mockBucket.Object)
                .Where(p => IntegerEnum.Value0 == p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 0)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NoEnumConverterOnNullable_WhereNotEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<IntegerNullableDoc>(mockBucket.Object)
                .Where(p => p.Value != IntegerEnum.Value0);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 0)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NoEnumConverterOnNullable_WhereNotEqualToNull()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<IntegerNullableDoc>(mockBucket.Object)
                .Where(p => p.Value != null);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` IS NOT NULL)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_NoEnumConverterOnNullable_WhereNotEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<IntegerNullableDoc>(mockBucket.Object)
                .Where(p => IntegerEnum.Value0 != p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 0)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region StringEnumConverter

        [Test]
        public void Test_StringEnumConverter_WhereEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringDoc>(mockBucket.Object)
                .Where(p => p.Value == StringEnum.Value0);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverter_WhereEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringDoc>(mockBucket.Object)
                .Where(p => StringEnum.Value0 == p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverter_WhereNotEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringDoc>(mockBucket.Object)
                .Where(p => p.Value != StringEnum.Value0);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverter_WhereEqualToValueWithEnumMemberAttribute()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringDoc>(mockBucket.Object)
                .Where(p => p.Value == StringEnum.Value1);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 'Value 1')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverter_WhereNotEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringDoc>(mockBucket.Object)
                .Where(p => StringEnum.Value0 != p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region StringEnumConverter on nullable value

        [Test]
        public void Test_StringEnumConverterOnNullable_WhereEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableDoc>(mockBucket.Object)
                .Where(p => p.Value == StringEnum.Value0);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnNullable_WhereEqualToNull()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableDoc>(mockBucket.Object)
                .Where(p => p.Value == null);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` IS NULL)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnNullable_WhereEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableDoc>(mockBucket.Object)
                .Where(p => StringEnum.Value0 == p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnNullable_WhereNotEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableDoc>(mockBucket.Object)
                .Where(p => p.Value != StringEnum.Value0);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnNullable_WhereNotEqualToNull()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableDoc>(mockBucket.Object)
                .Where(p => p.Value != null);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` IS NOT NULL)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnNullable_WhereNotEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableDoc>(mockBucket.Object)
                .Where(p => StringEnum.Value0 != p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region StringEnumConverter on property

        [Test]
        public void Test_StringEnumConverterOnProperty_WhereEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringOnPropertyDoc>(mockBucket.Object)
                .Where(p => p.Value == IntegerEnum.Value0);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnProperty_WhereEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringOnPropertyDoc>(mockBucket.Object)
                .Where(p => IntegerEnum.Value0 == p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnProperty_WhereNotEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringOnPropertyDoc>(mockBucket.Object)
                .Where(p => p.Value != IntegerEnum.Value0);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnProperty_WhereEqualToValueWithEnumMemberAttribute()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringOnPropertyDoc>(mockBucket.Object)
                .Where(p => p.Value == IntegerEnum.Value2);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 'Value 2')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnProperty_WhereNotEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringOnPropertyDoc>(mockBucket.Object)
                .Where(p => IntegerEnum.Value0 != p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region StringEnumConverter on property nullable value

        [Test]
        public void Test_StringEnumConverterOnPropertyNullable_WhereEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableOnPropertyDoc>(mockBucket.Object)
                .Where(p => p.Value == IntegerEnum.Value0);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnPropertyNullable_WhereEqualToNull()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableOnPropertyDoc>(mockBucket.Object)
                .Where(p => p.Value == null);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` IS NULL)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnPropertyNullable_WhereEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableOnPropertyDoc>(mockBucket.Object)
                .Where(p => IntegerEnum.Value0 == p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` = 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnPropertyNullable_WhereNotEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableOnPropertyDoc>(mockBucket.Object)
                .Where(p => p.Value != IntegerEnum.Value0);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnPropertyNullable_WhereNotEqualToNull()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableOnPropertyDoc>(mockBucket.Object)
                .Where(p => p.Value != null);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` IS NOT NULL)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringEnumConverterOnPropertyNullable_WhereNotEqualReversed()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = QueryFactory.Queryable<StringNullableOnPropertyDoc>(mockBucket.Object)
                .Where(p => IntegerEnum.Value0 != p.Value);

            const string expected =
                "SELECT RAW `Extent1` FROM `default` as `Extent1` WHERE (`Extent1`.`Value` != 'Value0')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Helpers

        private enum IntegerEnum
        {
            Value0 = 0,
            Value1 = 1,

            [EnumMember(Value="Value 2")]
            Value2 = 2
        }

        [JsonConverter(typeof(StringEnumConverter))]
        private enum StringEnum
        {
            Value0 = 0,

            [EnumMember(Value="Value 1")]
            Value1 = 1
        }

        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class IntegerDoc
        {
            public IntegerEnum Value { get; set; }
        }

        private class IntegerNullableDoc
        {
            public IntegerEnum? Value { get; set; }
        }

        private class StringDoc
        {
            public StringEnum Value { get; set; }
        }

        private class StringNullableDoc
        {
            public StringEnum? Value { get; set; }
        }

        private class StringOnPropertyDoc
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public IntegerEnum Value { get; set; }
        }

        private class StringNullableOnPropertyDoc
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public IntegerEnum? Value { get; set; }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        // ReSharper restore ClassNeverInstantiated.Local

        #endregion
    }
}
