using System;
using System.IO;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.ObjectStore.FileSystem;
using Shouldly;

namespace ServerlessMapReduceDotNet.Tests.UnitTests.ObjectStoreTests
{
    public class FileSystemObjectStoreTests : ObjectStoreTests
    {
        private string _tempPath;
        
        [SetUp]
        public void SetUp()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }
        
        protected override IObjectStore ObjectStoreFactoryImpl(ITime time)
        {
            return new FileSystemObjectStore(new FileSystem(), new FileSystemObjectStoreTestConfig(_tempPath), time);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_tempPath, true);
        }
    }

    public class AdditionalFileSystemObjectStoreTests
    {
        [Test]
        public async Task Given_that_a_DS_Store_file_is_in_dir__When_listing_dir__Then_DS_Store_is_not_returned()
        {
            // Arrange
            var fileSytem = Substitute.For<IFileSystem>();
            fileSytem.Directory_GetFiles(Arg.Is("/root/objectStore"), "*", SearchOption.AllDirectories)
                .Returns(new[]
                {
                    "/root/objectStore/dir/foo1",
                    "/root/objectStore/dir/.DS_Store",
                    "/root/objectStore/dir/foo2"
                });
            var fileSystemObjectStore = new FileSystemObjectStore(fileSytem, new FileSystemObjectStoreTestConfig("/root/objectStore/"), new Time());
            
            // Act
            var listedObjects = await fileSystemObjectStore.ListKeysPrefixedAsync("dir");

            // Assert
            listedObjects.Count.ShouldBe(2);
            listedObjects.ShouldContain(x => x.Key == "dir/foo1");
            listedObjects.ShouldContain(x => x.Key == "dir/foo2");
        }
    }
    
    internal class FileSystemObjectStoreTestConfig : IFileObjectStoreConfig
    {
        public FileSystemObjectStoreTestConfig(string rootFileObjectStorePath)
        {
            RootFileObjectStore = rootFileObjectStorePath;
        }
        
        public string RootFileObjectStore { get; }
    }
}