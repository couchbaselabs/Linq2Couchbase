using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Linq.Versioning;
using NUnit.Framework;
using Moq;

namespace Couchbase.Linq.IntegrationTests.Versioning
{
    [TestFixture]
    class DefaultVersionProviderTests
    {
        [Test]
        public void GetVersionNumber_ReturnsVersion()
        {
            // Arrange

            var provider = new DefaultVersionProvider();

            // Act

            var version = provider.GetVersion(ClusterHelper.GetBucket("beer-sample"));

            // Assert

            Assert.NotNull(version);
        }

        [Test]
        public void GetVersionNumber_NoDeadlock()
        {
            // Using an asynchronous HttpClient request within an MVC Web API action may cause
            // a deadlock when we wait for the result synchronously.

            var context = new Mock<SynchronizationContext>
            {
                CallBase = true
            };

            SynchronizationContext.SetSynchronizationContext(context.Object);
            try
            {
                var provider = new DefaultVersionProvider();

                provider.GetVersion(ClusterHelper.GetBucket("beer-sample"));

                // If view queries are incorrectly awaiting on the current SynchronizationContext
                // We will see calls to Post or Send on the mock

                context.Verify(m => m.Post(It.IsAny<SendOrPostCallback>(), It.IsAny<object>()), Times.Never);
                context.Verify(m => m.Send(It.IsAny<SendOrPostCallback>(), It.IsAny<object>()), Times.Never);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(null);
            }
        }
    }
}
