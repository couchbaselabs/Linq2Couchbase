using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Linq.Extensions;
using Couchbase.Linq.Tests.Documents;
using Couchbase;

namespace Couchbase.Linq.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var cluster = new Cluster())
            {
                using (var bucket = cluster.OpenBucket("tutorial"))
                {
                    var contacts = from c in bucket.Queryable<Contact>()
                        select c;

                    foreach (var contact in contacts)
                    {
                        Console.WriteLine("\tName={0}, Age={1}, Email={2}",
                            contact.FirstName,
                            contact.Age,
                            contact.Title);
                    }
                }
            }
            Console.Read();
        }























        void Example()
        {
           
        }
    }
}
