using System;
using System.Linq;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using NSubstitute;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.Services;
using ServerlessMapReduceDotNet.Tests.Builders;
using ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock;
using Shouldly;

namespace ServerlessMapReduceDotNet.Tests.UnitTests
{
    public class WorkerRecordStoreServiceTests
    {
        [Test]
        public async Task Given_an_empty_store__When_a_ping_is_recorded_for_worker1__Then_record_for_worker1_has_current_time_for_LastPingTime()
        {
            // Arrange
            var timeMock = Substitute.For<ITime>();
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-15 12:00"));
            var store = WorkerRecordStoreServiceFactory(timeMock);

            // Act
            await store.RecordPing("WorkerType1", "worker1");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.Count.ShouldBe(1);
            workerRecords.First().LastPingTime.ShouldBe(DateTime.Parse("2018-02-15 12:00"));
        }
        
        [Test]
        public async Task Given_a_ping_recorded_for_worker1__When_a_later_ping_is_recorded_for_worker1__Then_record_for_worker1_has_the_later_time_for_LastPingTime()
        {
            // Arrange
            var timeMock = Substitute.For<ITime>();
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-15 12:00"));
            var store = WorkerRecordStoreServiceFactory(timeMock);
            await store.RecordPing("WorkerType1", "worker1");
            
            // Act
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-15 13:00"));
            await store.RecordPing("WorkerType1", "worker1");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.Count.ShouldBe(1);
            workerRecords.First().LastPingTime.ShouldBe(DateTime.Parse("2018-02-15 13:00"));
        }
        
        [Test]
        public async Task Given_an_empty_store__When_a_ping_is_recorded_for_worker1__Then_record_for_worker1_has_true_ShouldRun()
        {
            // Arrange
            var store = WorkerRecordStoreServiceFactory();
            
            // Act
            await store.RecordPing("WorkerType1", "worker1");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.Count.ShouldBe(1);
            workerRecords.First().ShouldRun.ShouldBeTrue();
        }
        
        [Test]
        public async Task Given_an_empty_store__When_a_ping_is_recorded_for_worker1__Then_record_for_worker1_has_false_HasTerminated()
        {
            // Arrange
            var store = WorkerRecordStoreServiceFactory();
            
            // Act
            await store.RecordPing("WorkerType1", "worker1");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.Count.ShouldBe(1);
            workerRecords.First().HasTerminated.ShouldBeFalse();
        }
        
        [Test]
        public async Task Given_an_empty_store__When_a_worker1_should_stop__Then_record_for_worker1_has_false_ShouldRun()
        {
            // Arrange
            var store = WorkerRecordStoreServiceFactory();
            
            // Act
            await store.RecordShouldStop("WorkerType1", "worker1");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.Count.ShouldBe(1);
            workerRecords.First().ShouldRun.ShouldBeFalse();
        }
        
        [Test]
        public async Task Given_an_empty_store__When_a_worker1_should_stop__Then_record_for_worker1_has_min_value_LastPingTime()
        {
            // Arrange
            var store = WorkerRecordStoreServiceFactory();

            // Act
            await store.RecordShouldStop("WorkerType1", "worker1");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.Count.ShouldBe(1);
            workerRecords.First().LastPingTime.ShouldBe(DateTime.MaxValue);
        }
        
        [Test]
        public async Task Given_an_empty_store__When_a_worker1_should_stop__Then_record_for_worker1_has_false_HasTerminated()
        {
            // Arrange
            var store = WorkerRecordStoreServiceFactory();

            // Act
            await store.RecordShouldStop("WorkerType1", "worker1");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.Count.ShouldBe(1);
            workerRecords.First().HasTerminated.ShouldBeFalse();
        }
        
        [Test]
        public async Task Given_no_records_stored__When_worker_records_are_retrieved__Then_an_empty_list_is_returned()
        {
            // Arrange
            var store = WorkerRecordStoreServiceFactory();

            // Act
            var workerRecords = await store.GetAllWorkerRecords();

            // Assert
            workerRecords.Count.ShouldBe(0);
        }
        
        [Test]
        public async Task Given_a_ping_recorded_for_worker1__When_a_ping_is_recorded_for_worker2__Then_record_for_worker2_has_current_time_for_LastPingTime()
        {
            // Arrange
            var timeMock = Substitute.For<ITime>();
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-15 12:00"));
            var store = WorkerRecordStoreServiceFactory(timeMock);
            await store.RecordPing("WorkerType1", "worker1");

            // Act
            await store.RecordPing("WorkerType1", "worker2");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.First(x => x.Id == "worker2").LastPingTime.ShouldBe(DateTime.Parse("2018-02-15 12:00"));
        }
        
        [Test]
        public async Task Given_a_ping_recorded_for_worker1__When_a_ShouldStop_is_recorded_for_worker2__Then_record_for_worker1_has_true_ShouldRun()
        {
            // Arrange
            var timeMock = Substitute.For<ITime>();
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-15 12:00"));
            var store = WorkerRecordStoreServiceFactory(timeMock);
            await store.RecordPing("WorkerType1", "worker1");

            // Act
            await store.RecordShouldStop("WorkerType2", "worker2");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.First(x => x.Id == "worker1").ShouldRun.ShouldBeTrue();
        }
        
        [Test]
        public async Task Given_an_empty_store__When_a_HasTerminated_is_recorded_for_worker1__Then_record_for_worker1_has_current_time_for_LastPingTime()
        {
            // Arrange
            var timeMock = Substitute.For<ITime>();
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-15 12:00"));
            var store = WorkerRecordStoreServiceFactory(timeMock);

            // Act
            await store.RecordHasTerminated("WorkerType1", "worker1");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.Count.ShouldBe(1);
            workerRecords.First().LastPingTime.ShouldBe(DateTime.MaxValue);
        }
        
        [Test]
        public async Task Given_an_empty_store__When_a_HasTerminated_is_recorded_for_worker1__Then_record_for_worker1_has_false_HasTerminated()
        {
            // Arrange
            var store = WorkerRecordStoreServiceFactory();
            
            // Act
            await store.RecordHasTerminated("WorkerType1", "worker1");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.Count.ShouldBe(1);
            workerRecords.First().HasTerminated.ShouldBeTrue();
        }
        
        [Test]
        public async Task Given_an_empty_store__When_a_HasTerminated_is_recorded_for_worker1__Then_record_for_worker1_has_true_ShouldRun()
        {
            // Arrange
            var store = WorkerRecordStoreServiceFactory();
            
            // Act
            await store.RecordHasTerminated("WorkerType1", "worker1");

            // Assert
            var workerRecords = await store.GetAllWorkerRecords();
            workerRecords.Count.ShouldBe(1);
            workerRecords.First().ShouldRun.ShouldBeTrue();
        }
        
        [Test]
        public async Task Given_an_empty_worker_record_folder__When_getting_all_worker_records__Then_no_records_are_returned()
        {
            // Arrange
            var config = new ConfigBuiler().Build();
            var workerRecordFolder = config.WorkerRecordFolder;

            var timeMock = Substitute.For<ITime>();
            
            var commandDispatcher = Substitute.For<ICommandDispatcher>().RegisterMemoryObjectStore(timeMock);
            await commandDispatcher.DispatchAsync(new StoreObjectCommand
            {
                Key = workerRecordFolder,
                DataStream = StreamHelper.NewEmptyStream()
            });

            var store = WorkerRecordStoreServiceFactory(commandDispatcherMock: commandDispatcher, configMock: config, timeMock: timeMock);
            
            // Act
            var workerRecords = await store.GetAllWorkerRecords();

            // Assert
            workerRecords.Count.ShouldBe(0);
        }
        

        private IWorkerRecordStoreService WorkerRecordStoreServiceFactory(
            ITime timeMock = null,
            IConfig configMock = null,
            ICommandDispatcher commandDispatcherMock = null)
        {
            if (timeMock == null)
                timeMock = Substitute.For<ITime>();
            
            if (configMock == null)
                configMock = Substitute.For<IConfig>();

            if (commandDispatcherMock == null)
            {
                commandDispatcherMock = Substitute.For<ICommandDispatcher>()
                    .RegisterMemoryObjectStore(timeMock);
            }
            return new WorkerRecordStoreService(timeMock, configMock, commandDispatcherMock);
        }
        
        private T CheckParam<T>(T param) where T : class
        {
            return param ?? Substitute.For<T>();
        }
    }
}