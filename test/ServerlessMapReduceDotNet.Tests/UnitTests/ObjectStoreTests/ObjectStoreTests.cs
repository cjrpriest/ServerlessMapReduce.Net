using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Services;
using Shouldly;

namespace ServerlessMapReduceDotNet.Tests.UnitTests.ObjectStoreTests
{
    public abstract class ObjectStoreTests
    {
        [Test]
        public async Task Given_a_string_object_is_stored_in_area_key__When_object_is_retrieved_from_area_key__Then_the_stored_data_is_the_same()
        {
            // Arrange
            IObjectStore objectStore = ObjectStoreFactory();
            var inObjectStream = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
            await objectStore.StoreAsync("key", inObjectStream);
            
            // Act
            var outObjectStream = await objectStore.RetrieveAsync("key");

            // Assert
            ReadStringFromStream(outObjectStream).ShouldBe("hello");
        }
        
        [Test]
        public async Task Given_a_string_object_is_stored_in_area_key__When_object_is_retrieved_from_area_key2__Then_an_exception_should_be_thrown()
        {
            // Arrange
            IObjectStore objectStore = ObjectStoreFactory();
            var inObjectStream = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
            await objectStore.StoreAsync("key", inObjectStream);
            
            // Act / Assert
            await objectStore.RetrieveAsync("key2")
                .ShouldThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task Given_four_objects_stored_in_folder1_and_2__When_getting_keys_prefixed_folder2__Then_only_keys_in_folder2_are_returned()
        {
            // Arrang
            var timeMock = Substitute.For<ITime>();
            var objectStore = ObjectStoreFactory(timeMock);

            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-16 12:00"));
            await objectStore.StoreAsync("folder1/folderA/key1", StreamHelper.NewEmptyStream());
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-16 12:01"));
            await objectStore.StoreAsync("folder1/folderA/key2", StreamHelper.NewEmptyStream());
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-16 12:02"));
            await objectStore.StoreAsync("folder2/folderB/key1", StreamHelper.NewEmptyStream());
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-16 12:03"));
            await objectStore.StoreAsync("folder2/folderB/key2", StreamHelper.NewEmptyStream());

            // Act
            var foundObjectKeys = await objectStore.ListKeysPrefixedAsync("folder2/");

            // Assert
            foundObjectKeys.Count.ShouldBe(2);
            var key1 = foundObjectKeys.FirstOrDefault(x => x.Key == "folder2/folderB/key1");
            var key2 = foundObjectKeys.FirstOrDefault(x => x.Key == "folder2/folderB/key2");
            key1.ShouldNotBeNull();
            key2.ShouldNotBeNull();
            key1.LastModified.ShouldBe(DateTime.Parse("2018-02-16 12:02"));
            key2.LastModified.ShouldBe(DateTime.Parse("2018-02-16 12:03"));
        }
        
        [Test]
        public async Task Given_an_object_stored_in_BST__When_getting_keys_by_prefix__Then_object_time_is_returned_in_UTC()
        {
            // Arrang
            var timeMock = Substitute.For<ITime>();
            var objectStore = ObjectStoreFactory(timeMock);

            timeMock.UtcNow.Returns(DateTime.Parse("2017-06-16 12:00"));
            await objectStore.StoreAsync("folder1/folderA/key1", StreamHelper.NewEmptyStream());

            // Act
            var foundObjectKeys = await objectStore.ListKeysPrefixedAsync("folder1/");

            // Assert
            foundObjectKeys.Count.ShouldBe(1);
            foundObjectKeys.First().LastModified.ShouldBe(DateTime.Parse("2017-06-16 12:00"));
        }

        private string ReadStringFromStream(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        private IObjectStore ObjectStoreFactory(ITime time = null)
        {
            if (time == null)
                time = Substitute.For<ITime>();
            return ObjectStoreFactoryImpl(time);
        }

        protected abstract IObjectStore ObjectStoreFactoryImpl(ITime time);
    }
}