using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Linq.Versioning;
using Moq;
using NUnit.Framework;

namespace Couchbase.Linq.UnitTests.Versioning
{
    [TestFixture]
    public class DefaultVersionProviderTests
    {
        private static readonly Version Version40 = new Version(4, 0, 0);
        private static readonly Version Version45 = new Version(4, 5, 0);

        private static readonly Uri Uri1 = new Uri("http://abc.def");
        private static readonly Uri Uri2 = new Uri("http://def.abc");

        #region GetVersion

        [Test]
        public void GetVersion_Succeeds_ReturnsVersion()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Configuration)
                .Returns(new BucketConfiguration()
                {
                    PoolConfiguration = new PoolConfiguration()
                    {
                        ClientConfiguration = new ClientConfiguration()
                        {
                            Servers = new List<Uri>
                            {
                                Uri1
                            }
                        }
                    }
                });

            var provider = new Mock<DefaultVersionProvider>()
            {
                CallBase = true
            };

            provider
                .Setup(m => m.DownloadConfig(It.IsAny<Uri>()))
                .ReturnsAsync(new DefaultVersionProvider.Bootstrap()
                {
                    ImplementationVersion = "4.5.0"
                });

            // Act

            var result = provider.Object.GetVersion(bucket.Object);

            // Assert

            Assert.AreEqual(Version45, result);
        }

        [Test]
        public void GetVersion_Succeeds_CachesResult()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Configuration)
                .Returns(new BucketConfiguration()
                {
                    PoolConfiguration = new PoolConfiguration()
                    {
                        ClientConfiguration = new ClientConfiguration()
                        {
                            Servers = new List<Uri>
                            {
                                Uri1,
                                Uri2
                            }
                        }
                    }
                });

            var provider = new Mock<DefaultVersionProvider>()
            {
                CallBase = true
            };

            provider
                .Setup(m => m.DownloadConfig(It.IsAny<Uri>()))
                .ReturnsAsync(new DefaultVersionProvider.Bootstrap()
                {
                    ImplementationVersion = "4.5.0"
                });

            // Act

            provider.Object.GetVersion(bucket.Object);

            // Assert

            provider
                .Verify(
                    m => m.CacheStore(It.Is<List<Uri>>(p => p.Contains(Uri1) && p.Contains(Uri2)), Version45),
                    Times.Once);
        }

        [Test]
        public void GetVersion_Fails_Returns4()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Configuration)
                .Returns(new BucketConfiguration()
                {
                    PoolConfiguration = new PoolConfiguration()
                    {
                        ClientConfiguration = new ClientConfiguration()
                        {
                            Servers = new List<Uri>
                            {
                                Uri1
                            }
                        }
                    }
                });

            var provider = new Mock<DefaultVersionProvider>()
            {
                CallBase = true
            };

            provider
                .Setup(m => m.DownloadConfig(It.IsAny<Uri>()))
                .ReturnsAsync(null);

            // Act

            var result = provider.Object.GetVersion(bucket.Object);

            // Assert

            Assert.AreEqual(Version40, result);
        }

        [Test]
        public void GetVersion_DownloadThrowsException_Returns4()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Configuration)
                .Returns(new BucketConfiguration()
                {
                    PoolConfiguration = new PoolConfiguration()
                    {
                        ClientConfiguration = new ClientConfiguration()
                        {
                            Servers = new List<Uri>
                            {
                                Uri1
                            }
                        }
                    }
                });

            var provider = new Mock<DefaultVersionProvider>()
            {
                CallBase = true
            };

            provider
                .Setup(m => m.DownloadConfig(It.IsAny<Uri>()))
                .ThrowsAsync(new ApplicationException());

            // Act

            var result = provider.Object.GetVersion(bucket.Object);

            // Assert

            Assert.AreEqual(Version40, result);
        }

        [Test]
        public void GetVersion_Fails_CachesResult()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Configuration)
                .Returns(new BucketConfiguration()
                {
                    PoolConfiguration = new PoolConfiguration()
                    {
                        ClientConfiguration = new ClientConfiguration()
                        {
                            Servers = new List<Uri>
                            {
                                Uri1,
                                Uri2
                            }
                        }
                    }
                });

            var provider = new Mock<DefaultVersionProvider>()
            {
                CallBase = true
            };

            provider
                .Setup(m => m.DownloadConfig(It.IsAny<Uri>()))
                .ReturnsAsync(null);

            // Act

            provider.Object.GetVersion(bucket.Object);

            // Assert

            provider
                .Verify(
                    m => m.CacheStore(It.Is<List<Uri>>(p => p.Contains(Uri1) && p.Contains(Uri2)), Version40),
                    Times.Once);
        }

        [Test]
        public void GetVersion_FirstDownloadFails_ReturnsSecond()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Configuration)
                .Returns(new BucketConfiguration()
                {
                    PoolConfiguration = new PoolConfiguration()
                    {
                        ClientConfiguration = new ClientConfiguration()
                        {
                            Servers = new List<Uri>
                            {
                                Uri1,
                                Uri2
                            }
                        }
                    }
                });

            var provider = new Mock<DefaultVersionProvider>()
            {
                CallBase = true
            };

            provider
                .Setup(m => m.Shuffle(It.IsAny<List<Uri>>()))
                .Returns((List<Uri> p1) => p1);
            provider
                .Setup(m => m.DownloadConfig(Uri1))
                .ReturnsAsync(null);
            provider
                .Setup(m => m.DownloadConfig(Uri2))
                .ReturnsAsync(new DefaultVersionProvider.Bootstrap()
                {
                    ImplementationVersion = "4.5.0"
                });

            // Act

            var result = provider.Object.GetVersion(bucket.Object);

            // Assert

            Assert.AreEqual(Version45, result);
        }

        [Test]
        public void GetVersion_FirstDownloadThrowsException_ReturnsSecond()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket
                .SetupGet(m => m.Configuration)
                .Returns(new BucketConfiguration()
                {
                    PoolConfiguration = new PoolConfiguration()
                    {
                        ClientConfiguration = new ClientConfiguration()
                        {
                            Servers = new List<Uri>
                            {
                                Uri1,
                                Uri2
                            }
                        }
                    }
                });

            var provider = new Mock<DefaultVersionProvider>()
            {
                CallBase = true
            };

            provider
                .Setup(m => m.Shuffle(It.IsAny<List<Uri>>()))
                .Returns((List<Uri> p1) => p1);
            provider
                .Setup(m => m.DownloadConfig(Uri1))
                .ThrowsAsync(new ApplicationException());
            provider
                .Setup(m => m.DownloadConfig(Uri2))
                .ReturnsAsync(new DefaultVersionProvider.Bootstrap()
                {
                    ImplementationVersion = "4.5.0"
                });

            // Act

            var result = provider.Object.GetVersion(bucket.Object);

            // Assert

            Assert.AreEqual(Version45, result);
        }

        #endregion

        #region CacheLookup/CacheStore

        [Test]
        public void CacheLookup_EmptyCache_ReturnsNull()
        {
            // Arrange

            var servers = new List<Uri>
            {
                Uri1
            };

            var provider = new DefaultVersionProvider();

            // Act

            var result = provider.CacheLookup(servers);

            // Assert

            Assert.IsNull(result);
        }

        [Test]
        public void CacheLookup_IsInCache_ReturnsVersion()
        {
            // Arrange

            var servers = new List<Uri>
            {
                Uri1
            };

            var provider = new DefaultVersionProvider();

            provider.CacheStore(servers, Version45);

            // Act

            var result = provider.CacheLookup(servers);

            // Assert

            Assert.AreEqual(Version45, result);
        }

        [Test]
        public void CacheLookup_MultipleUrisOneInCache_ReturnsVersion()
        {
            // Arrange

            var servers1 = new List<Uri>
            {
                Uri1
            };

            var servers2 = new List<Uri>
            {
                Uri2
            };

            var provider = new DefaultVersionProvider();

            provider.CacheStore(servers2, Version45);

            // Act

            var result = provider.CacheLookup(servers1.Concat(servers2));

            // Assert

            Assert.AreEqual(Version45, result);
        }

        [Test]
        public void CacheStore_MultipleUris_StoresAll()
        {
            // Arrange

            var servers = new List<Uri>
            {
                Uri1,
                Uri2
            };

            var provider = new DefaultVersionProvider();

            // Act

            provider.CacheStore(servers, Version45);

            // Assert

            var result = provider.CacheLookup(servers.Take(1));
            Assert.AreEqual(Version45, result);

            result = provider.CacheLookup(servers.Skip(1));
            Assert.AreEqual(Version45, result);
        }

        #endregion

        #region ExtractVersion

        [Test]
        public void ExtractVersion_NullConfig_ReturnsNull()
        {
            // Arrange

            var provider = new DefaultVersionProvider();

            // Act

            var result = provider.ExtractVersion(null);

            // Assert

            Assert.IsNull(result);
        }

        [Test]
        public void ExtractVersion_EmptyVersion_ReturnsNull()
        {
            // Arrange

            var provider = new DefaultVersionProvider();

            // Act

            var result = provider.ExtractVersion(new DefaultVersionProvider.Bootstrap()
            {
                ImplementationVersion = ""
            });

            // Assert

            Assert.IsNull(result);
        }

        [Test]
        public void ExtractVersion_VersionWithoutDash_ReturnsVersion()
        {
            // Arrange

            var provider = new DefaultVersionProvider();

            // Act

            var result = provider.ExtractVersion(new DefaultVersionProvider.Bootstrap()
            {
                ImplementationVersion = "4.5.0"
            });

            // Assert

            Assert.AreEqual(Version45, result);
        }

        [Test]
        public void ExtractVersion_VersionWithDash_ReturnsVersion()
        {
            // Arrange

            var provider = new DefaultVersionProvider();

            // Act

            var result = provider.ExtractVersion(new DefaultVersionProvider.Bootstrap()
            {
                ImplementationVersion = "4.5.0-somethingelse"
            });

            // Assert

            Assert.AreEqual(Version45, result);
        }

        [Test]
        public void ExtractVersion_InvalidVersion_ReturnsNull()
        {
            // Arrange

            var provider = new DefaultVersionProvider();

            // Act

            var result = provider.ExtractVersion(new DefaultVersionProvider.Bootstrap()
            {
                ImplementationVersion = "4.5.0a"
            });

            // Assert

            Assert.IsNull(result);
        }

        #endregion
    }
}
