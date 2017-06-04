using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq.QueryGeneration;
using Couchbase.Linq.QueryGeneration.Expressions;
using Couchbase.Linq.QueryGeneration.MemberNameResolvers;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;

// ReSharper disable StringCompareIsCultureSpecific.1
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable StringIndexOfIsCultureSpecific.1
namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    class StringTests : N1QLTestBase
    {

        #region Literal Tests

        [Test]
        public void Test_StringLiteral()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where contact.FirstName == "M"
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` = 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringLiteralWithQuote()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where contact.FirstName == "M'"
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` = 'M''')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_CharLiteral()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { contact.FirstName, ch = 'M' };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName`, 'M' as `ch` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_CharLiteral_Quote()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { contact.FirstName, ch = '\'' };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName`, '''' as `ch` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Function Tests

        [Test]
        public void Test_StringLength()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where contact.FirstName.Length > 5
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (LENGTH(`Extent1`.`fname`) > 5)";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ToUpper()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where contact.FirstName.ToUpper() == "BOB"
                        select new { contact.LastName };

            const string expected =
                "SELECT `Extent1`.`lname` as `LastName` FROM `default` as `Extent1` " +
                "WHERE (UPPER(`Extent1`.`fname`) = 'BOB')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_ToLower()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where contact.FirstName.ToLower() == "bob"
                        select new { contact.LastName };

            const string expected =
                "SELECT `Extent1`.`lname` as `LastName` FROM `default` as `Extent1` " +
                "WHERE (LOWER(`Extent1`.`fname`) = 'bob')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Substring_NoLength()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.Substring(1) };

            const string expected =
                "SELECT SUBSTR(`Extent1`.`fname`, 1) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Substring_WithLength()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.Substring(1, 5) };

            const string expected =
                "SELECT SUBSTR(`Extent1`.`fname`, 1, 5) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Substring_CharIndex()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { firstLetter = contact.FirstName[0] };

            const string expected =
                "SELECT SUBSTR(`Extent1`.`fname`, 0, 1) as `firstLetter` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Split_NullCharacters()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.Split(null) };

            const string expected =
                "SELECT SPLIT(`Extent1`.`fname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Split_EmptyCharacters()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.Split() };

            const string expected =
                "SELECT SPLIT(`Extent1`.`fname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Split_HasCharacter()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.Split(' ') };

            const string expected =
                "SELECT SPLIT(`Extent1`.`fname`, ' ') as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Split_HasMultipleCharacters()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.Split(' ', '\t') };

            Assert.Throws<NotSupportedException>(() => CreateN1QlQuery(mockBucket.Object, query.Expression));
        }

        [Test]
        public void Test_IndexOf_Character()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { index = contact.FirstName.IndexOf(' ') };

            const string expected =
                "SELECT POSITION(`Extent1`.`fname`, ' ') as `index` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IndexOf_String()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { index = contact.FirstName.IndexOf(" ") };

            const string expected =
                "SELECT POSITION(`Extent1`.`fname`, ' ') as `index` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Replace()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.Replace(" ", "") };

            const string expected =
                "SELECT REPLACE(`Extent1`.`fname`, ' ', '') as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Trim_NoChars()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.Trim() };

            const string expected =
                "SELECT TRIM(`Extent1`.`fname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Trim_NullChars()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.Trim(null) };

            const string expected =
                "SELECT TRIM(`Extent1`.`fname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_Trim_WithChars()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.Trim(' ', '\t', '\'') };

            const string expected =
                "SELECT TRIM(`Extent1`.`fname`, ' \t''') as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_TrimEnd_NoChars()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.TrimEnd() };

            const string expected =
                "SELECT RTRIM(`Extent1`.`fname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_TrimEnd_NullChars()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.TrimEnd(null) };

            const string expected =
                "SELECT RTRIM(`Extent1`.`fname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_TrimEnd_WithChars()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.TrimEnd(' ', '\t', '\'') };

            const string expected =
                "SELECT RTRIM(`Extent1`.`fname`, ' \t''') as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_TrimStart_NoChars()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.TrimStart() };

            const string expected =
                "SELECT LTRIM(`Extent1`.`fname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_TrimStart_NullChars()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.TrimStart(null) };

            const string expected =
                "SELECT LTRIM(`Extent1`.`fname`) as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_TrimStart_WithChars()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { name = contact.FirstName.TrimStart(' ', '\t', '\'') };

            const string expected =
                "SELECT LTRIM(`Extent1`.`fname`, ' \t''') as `name` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region ToString Tests

        [Test]
        public void Test_StringToString()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { FirstName = contact.FirstName.ToString() };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_IntegerToString()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        select new { Age = contact.Age.ToString() };

            const string expected =
                "SELECT TOSTRING(`Extent1`.`age`) as `Age` FROM `default` as `Extent1`";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion

        #region Compare N1QL Format Tests

        [Test]
        public void Test_StringCompare_Equal()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where string.Compare(contact.FirstName, "M") == 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` = 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompare_EqualOrdinal()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where string.Compare(contact.FirstName, "M", StringComparison.Ordinal) == 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` = 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompare_EqualOrginalIgnoreCase()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where string.Compare(contact.FirstName, "M", StringComparison.OrdinalIgnoreCase) == 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (LOWER(`Extent1`.`fname`) = LOWER('M'))";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompare_EqualCurrCultureIgnoreCase()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where string.Compare(contact.FirstName, "M", StringComparison.CurrentCultureIgnoreCase) == 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (LOWER(`Extent1`.`fname`) = LOWER('M'))";

            Assert.Throws<NotSupportedException>(() => CreateN1QlQuery(mockBucket.Object, query.Expression));
        }

        [Test]
        public void Test_StringCompare_EqualCurrCultureCase()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where string.Compare(contact.FirstName, "M", StringComparison.CurrentCulture) == 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (LOWER(`Extent1`.`fname`) = LOWER('M'))";

            Assert.Throws<NotSupportedException>(() => CreateN1QlQuery(mockBucket.Object, query.Expression));
        }

        [Test]
        public void Test_StringCompare_EqualCurrCulturegnoreCase()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where string.Compare(contact.FirstName, "M", StringComparison.CurrentCultureIgnoreCase) == 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (LOWER(`Extent1`.`fname`) = LOWER('M'))";

            Assert.Throws<NotSupportedException>(() => CreateN1QlQuery(mockBucket.Object, query.Expression));
        }

        [Test]
        public void Test_StringCompare_NotEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where string.Compare(contact.FirstName, "M") != 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` != 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompare_LessThan()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where string.Compare(contact.FirstName, "M") < 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` < 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompare_LessThanOrEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where string.Compare(contact.FirstName, "M") <= 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` <= 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompare_GreaterThan()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where string.Compare(contact.FirstName, "M") > 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` > 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompare_GreaterThanOrEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where string.Compare(contact.FirstName, "M") >= 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` >= 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompareTo_Equal()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where contact.FirstName.CompareTo("M") == 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` = 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompareTo_NotEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where contact.FirstName.CompareTo("M") != 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` != 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompareTo_LessThan()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where contact.FirstName.CompareTo("M") < 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` < 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompareTo_LessThanOrEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where contact.FirstName.CompareTo("M") <= 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` <= 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompareTo_GreaterThan()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where contact.FirstName.CompareTo("M") > 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` > 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        [Test]
        public void Test_StringCompareTo_GreaterThanOrEqual()
        {
            var mockBucket = new Mock<IBucket>();
            mockBucket.SetupGet(e => e.Name).Returns("default");

            var query = from contact in QueryFactory.Queryable<Contact>(mockBucket.Object)
                        where contact.FirstName.CompareTo("M") >= 0
                        select new { contact.FirstName };

            const string expected =
                "SELECT `Extent1`.`fname` as `FirstName` FROM `default` as `Extent1` " +
                "WHERE (`Extent1`.`fname` >= 'M')";

            var n1QlQuery = CreateN1QlQuery(mockBucket.Object, query.Expression);

            Assert.AreEqual(expected, n1QlQuery);
        }

        #endregion
    }
}
