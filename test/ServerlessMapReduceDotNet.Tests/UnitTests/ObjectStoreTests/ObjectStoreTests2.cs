using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using NSubstitute;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.ObjectStore;
using ServerlessMapReduceDotNet.Services;
using Shouldly;

namespace ServerlessMapReduceDotNet.Tests.UnitTests.ObjectStoreTests
{
    public abstract class ObjectStoreTests2
    {
        [Test]
        public async Task Given_a_string_object_is_stored_in_area_key__When_object_is_retrieved_from_area_key__Then_the_stored_data_is_the_same()
        {
            // Arrange
            var commandDispatcher = CommandDispatcherFactory();
            var inObjectStream = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
            await commandDispatcher.DispatchAsync(new StoreObjectCommand {DataStream = inObjectStream, Key = "key"});
            
            // Act
            var outObjectStream2 = await commandDispatcher.DispatchAsync(new RetrieveObjectCommand{ Key = "key" });

            // Assert
            ReadStringFromStream(outObjectStream2).ShouldBe("hello");
        }
        
        [Test]
        public async Task Given_a_string_object_is_stored_in_area_key__When_object_is_retrieved_from_area_key2__Then_an_exception_should_be_thrown()
        {
            // Arrange
            var commandDispatcher = CommandDispatcherFactory();
            var inObjectStream = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
            await commandDispatcher.DispatchAsync(new StoreObjectCommand {DataStream = inObjectStream, Key = "key"});

            // Act / Assert
            await commandDispatcher.DispatchAsync(new RetrieveObjectCommand {Key = "key2"})
                .ShouldThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task Given_four_objects_stored_in_folder1_and_2__When_getting_keys_prefixed_folder2__Then_only_keys_in_folder2_are_returned()
        {
            // Arrange
            var timeMock = Substitute.For<ITime>();
            var commandDispatcher = CommandDispatcherFactory(timeMock);

            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-16 12:00"));
            await commandDispatcher.DispatchAsync(new StoreObjectCommand {DataStream = StreamHelper.NewEmptyStream(), Key = "folder1/folderA/key1"});
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-16 12:01"));
            await commandDispatcher.DispatchAsync(new StoreObjectCommand {DataStream = StreamHelper.NewEmptyStream(), Key = "folder1/folderA/key2"});
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-16 12:02"));
            await commandDispatcher.DispatchAsync(new StoreObjectCommand {DataStream = StreamHelper.NewEmptyStream(), Key = "folder2/folderB/key1"});
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-16 12:03"));
            await commandDispatcher.DispatchAsync(new StoreObjectCommand {DataStream = StreamHelper.NewEmptyStream(), Key = "folder2/folderB/key2"});

            // Act
            IReadOnlyCollection<ListedObject> foundObjectKeys = (await commandDispatcher.DispatchAsync(new ListObjectKeysCommand {Prefix = "folder2/"})).Result;

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
            // Arrange
            var timeMock = Substitute.For<ITime>();
            var commandDispatcher = CommandDispatcherFactory(timeMock);

            timeMock.UtcNow.Returns(DateTime.Parse("2017-06-16 12:00"));
            await commandDispatcher.DispatchAsync(new StoreObjectCommand {DataStream = StreamHelper.NewEmptyStream(), Key = "folder1/folderA/key1"});

            // Act
            IReadOnlyCollection<ListedObject> foundObjectKeys = (await commandDispatcher.DispatchAsync(new ListObjectKeysCommand {Prefix = "folder1/"})).Result;

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
            return null;
        }

        private ICommandDispatcher CommandDispatcherFactory(ITime time = null)
        {
            var commandDispatcher = Substitute.For<ICommandDispatcher>();
            RegisterObjectStore(commandDispatcher, time);
            return commandDispatcher;
        }

        protected abstract void RegisterObjectStore(ICommandDispatcher commandDispatcher, ITime time);
    }
}