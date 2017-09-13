using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Core.Version;
using Couchbase.Linq.Versioning;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.Versioning
{
    [TestFixture]
    public class DefaultVersionProviderTests
    {
        private static readonly ClusterVersion Version40 = new ClusterVersion(new Version(4, 0, 0));
        private static readonly ClusterVersion Version45 = new ClusterVersion(new Version(4, 5, 0));

        private static readonly Uri Uri1 = new Uri("http://abc.def");
        private static readonly Uri Uri2 = new Uri("http://def.abc");

        #region GetVersion

        [Test]
        public void GetVersion_Succeeds_ReturnsVersion()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Cluster)
                .Returns(new Mock<ICluster>().Object);
            bucket
                .Setup(m => m.GetClusterVersionAsync())
                .Returns(Task.FromResult<ClusterVersion?>(Version45));

            var provider = new DefaultVersionProvider();

            // Act

            var result = provider.GetVersion(bucket.Object);

            // Assert

            Assert.AreEqual(Version45, result);
        }

        [Test]
        public void GetVersion_Succeeds_CachesResult()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Cluster)
                .Returns(new Mock<ICluster>().Object);
            bucket
                .Setup(m => m.GetClusterVersionAsync())
                .Returns(Task.FromResult<ClusterVersion?>(Version45));

            var provider = new Mock<DefaultVersionProvider>()
            {
                CallBase = true
            };

            // Act

            provider.Object.GetVersion(bucket.Object);

            // Assert

            provider
                .Verify(
                    m => m.CacheStore(bucket.Object.Cluster, Version45),
                    Times.Once);
        }

        [Test]
        public void GetVersion_Fails_Returns4()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Cluster)
                .Returns(new Mock<ICluster>().Object);
            bucket
                .Setup(m => m.GetClusterVersionAsync())
                .Returns(Task.FromResult<ClusterVersion?>(null));

            var provider = new DefaultVersionProvider();

            // Act

            var result = provider.GetVersion(bucket.Object);

            // Assert

            Assert.AreEqual(Version40, result);
        }

        [Test]
        public void GetVersion_DownloadThrowsException_Returns4()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Cluster)
                .Returns(new Mock<ICluster>().Object);
            bucket
                .Setup(m => m.GetClusterVersionAsync())
                .Returns(Task.FromException<ClusterVersion?>(new Exception()));

            var provider = new DefaultVersionProvider();

            // Act

            var result = provider.GetVersion(bucket.Object);

            // Assert

            Assert.AreEqual(Version40, result);
        }

        [Test]
        public void GetVersion_Fails_CachesResult()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Cluster)
                .Returns(new Mock<ICluster>().Object);
            bucket
                .Setup(m => m.GetClusterVersionAsync())
                .Returns(Task.FromResult<ClusterVersion?>(null));

            var provider = new Mock<DefaultVersionProvider>
            {
                CallBase = true
            };

            // Act

            provider.Object.GetVersion(bucket.Object);

            // Assert

            provider
                .Verify(
                    m => m.CacheStore(bucket.Object.Cluster, Version40),
                    Times.Once);
        }

        #endregion

        #region CacheLookup/CacheStore

        [Test]
        public void CacheLookup_EmptyCache_ReturnsNull()
        {
            // Arrange

            var servers = new Mock<ICluster>();

            var provider = new DefaultVersionProvider();

            // Act

            var result = provider.CacheLookup(servers.Object);

            // Assert

            Assert.IsNull(result);
        }

        [Test]
        public void CacheLookup_IsInCache_ReturnsVersion()
        {
            // Arrange

            var cluster = new Mock<ICluster>();

            var provider = new DefaultVersionProvider();

            provider.CacheStore(cluster.Object, Version45);

            // Act

            var result = provider.CacheLookup(cluster.Object);

            // Assert

            Assert.AreEqual(Version45, result);
        }

        #endregion

    }
}
