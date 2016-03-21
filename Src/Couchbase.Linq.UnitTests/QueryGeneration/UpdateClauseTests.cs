using System;
using System.Linq;
using Couchbase.Core;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.UnitTests.Documents;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;

namespace Couchbase.Linq.UnitTests.QueryGeneration
{
    [TestFixture]
    public class UpdateClauseTests : N1QLTestBase
    {
        [Test]
        public void Test_Update_UseKeys()
        {
            CreateQueryable<Contact>("default")
                    .UseKeys(new[] { "keyname" })
                    .Set(x => x.Age == 5)
                    .ToArray();

            const string expected =
                "UPDATE `default` as `Extent1` USE KEYS ['keyname'] SET `Extent1`.`age` = 5 RETURNING `Extent1`.*";
            Assert.AreEqual(expected, QueryExecutor.Query);
        }

        [Test]
        public void Test_Update_Condition()
        {
            CreateQueryable<Contact>("default")
                    .Where(e => e.Age > 10)
                    .Set(x => x.Age == 5)
                    .ToArray();

            const string expected =
                "UPDATE `default` as `Extent1` SET `Extent1`.`age` = 5 WHERE (`Extent1`.`age` > 10) RETURNING `Extent1`.*";
            Assert.AreEqual(expected, QueryExecutor.Query);
        }

        [Test]
        public void Test_Unset_Condition()
        {
            CreateQueryable<Contact>("default")
                    .Where(e => e.Age > 10)
                    .Unset(x => x.Age)
                    .ToArray();

            const string expected =
                "UPDATE `default` as `Extent1` UNSET `Extent1`.`age` WHERE (`Extent1`.`age` > 10) RETURNING `Extent1`.*";
            Assert.AreEqual(expected, QueryExecutor.Query);
        }

        [Test]
        public void Test_ChainUpdate()
        {
            CreateQueryable<Contact>("default")
                    .Where(e => e.Age > 10)
                    .Set(x => x.Age == 5 && x.Email == x.FirstName && x.LastName == "lastname")
                    .ToArray();

            const string expected =
                "UPDATE `default` as `Extent1` SET `Extent1`.`age` = 5, `Extent1`.`email` = `Extent1`.`fname`, `Extent1`.`lname` = 'lastname' WHERE (`Extent1`.`age` > 10) RETURNING `Extent1`.*";
            Assert.AreEqual(expected, QueryExecutor.Query);
        }

        [Test]
        public void Test_Update_SetAndUnset_WithSelectAndCondition()
        {
            CreateQueryable<Contact>("default")
                    .Where(e => e.Age > 10 && e.FirstName == "Sam")
                    .OrderBy(e => e.Age)
                    .Set(x => x.Age == 5)
                    .Set(x => x.Email == x.FirstName)
                    .Unset(x=>x.Title)
                    .Select(e => new { age = e.Age, name = e.FirstName })
                    .ToArray();

            const string expected =
                "UPDATE `default` as `Extent1` SET `Extent1`.`age` = 5, `Extent1`.`email` = `Extent1`.`fname` UNSET `Extent1`.`title` WHERE ((`Extent1`.`age` > 10) AND (`Extent1`.`fname` = 'Sam')) RETURNING `Extent1`.`age` as `age`, `Extent1`.`fname` as `name`";
            Assert.AreEqual(expected, QueryExecutor.Query);
        }

        [Test]
        public void Test_Update_WithNoSelection()
        {
            CreateQueryable<Contact>("default")
                    .Where(e => e.Age > 10 && e.FirstName == "Sam")
                    .OrderBy(e => e.Age)
                    .Set(x => x.Age == 5)
                    .Set(x => x.Email == x.FirstName)
                    .Unset(x => x.Title)
                    .Execute();

            const string expected =
                "UPDATE `default` as `Extent1` SET `Extent1`.`age` = 5, `Extent1`.`email` = `Extent1`.`fname` UNSET `Extent1`.`title` WHERE ((`Extent1`.`age` > 10) AND (`Extent1`.`fname` = 'Sam'))";
            Assert.AreEqual(expected, QueryExecutor.Query);
        }
    }
}