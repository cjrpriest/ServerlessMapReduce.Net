using System;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using NSubstitute;
using NUnit.Framework;
using ServerlessMapReduceDotNet.MapReduce.Commands.FinalReduce;
using ServerlessMapReduceDotNet.MapReduce.Commands.Ingest;
using ServerlessMapReduceDotNet.MapReduce.Commands.Map;
using ServerlessMapReduceDotNet.MapReduce.FireAndForgetFunctions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using ServerlessMapReduceDotNet.Tests.Builders;

namespace ServerlessMapReduceDotNet.Tests.UnitTests
{
    public class WorkerManagerTests
    {
        [TestCase(10, 1, 0, 1)]
        [TestCase(10, 0, 0, 0)]
        [TestCase(10, 0, 1, 0)]
        [TestCase(10, 9, 0, 1)]
        [TestCase(10, 10, 0, 1)]
        [TestCase(10, 11, 0, 2)]
        [TestCase(10, 19, 0, 2)]
        [TestCase(10, 20, 0, 2)]
        [TestCase(10, 21, 0, 3)]
        [TestCase(10, 10, 1, 0)]
        [TestCase(10, 20, 1, 1)]
        [TestCase(10, 20, 2, 0)]
        [TestCase(10, 21, 2, 1)]
        public async Task Given_there_are_x_message_on_the_raw_data_queue_and_y_running_ingesters__When_worker_manager_is_run__Then_z_new_ingesters_are_invoked(
            int inputQueueItemsPerRunningWorker,
            int inputNoOfMessages,
            int inputNoOfRunningIngesters,
            int outputExpectedNewIngestersInvoked)
        {
            // Arrange
            var commandDispatcherMock = Substitute.For<ICommandDispatcher>();

            var configMock = new ConfigBuiler()
                .With(
                    rawDataQueueName: "rawQ",
                    queueItemsPerRunningWorker: inputQueueItemsPerRunningWorker)
                .Build();
            
            var queueClientMock = new QueueClientMockBuilder()
                .WithRandomMessages(configMock.RawDataQueueName, inputNoOfMessages)
                .Build();
            
            var timeMock = Substitute.For<ITime>();
            timeMock.UtcNow.Returns(DateTime.Parse("2000-01-01 12:00"));

            var workerRecordStoreServiceMock = new WorkerRecordStoreServiceMockBuilder()
                .WithWorkerRecords(inputNoOfRunningIngesters, "ingester", timeMock.UtcNow)
                .Build();
            
            var workerManager = WorkerManagerFactory(
                commandDispatcher: commandDispatcherMock,
                config: configMock,
                queueClient: queueClientMock,
                workerRecordStoreService: workerRecordStoreServiceMock,
                time: timeMock);

            // Act
            await workerManager.InvokeAsync();

            // Assert
            await commandDispatcherMock.Received(outputExpectedNewIngestersInvoked)
                .DispatchAsync(Arg.Any<IngestCommand>());
        }
        
        [TestCase(10, 1, 0, 1)]
        [TestCase(10, 0, 0, 0)]
        [TestCase(10, 0, 1, 0)]
        [TestCase(10, 9, 0, 1)]
        [TestCase(10, 10, 0, 1)]
        [TestCase(10, 11, 0, 2)]
        [TestCase(10, 19, 0, 2)]
        [TestCase(10, 20, 0, 2)]
        [TestCase(10, 21, 0, 3)]
        [TestCase(10, 10, 1, 0)]
        [TestCase(10, 20, 1, 1)]
        [TestCase(10, 20, 2, 0)]
        [TestCase(10, 21, 2, 1)]
        public async Task Given_there_are_x_message_on_the_ingested_queue_and_y_running_mappers__When_worker_manager_is_run__Then_z_new_mappers_are_invoked(
            int inputQueueItemsPerRunningWorker,
            int inputNoOfMessages,
            int inputNoOfRunningIngesters,
            int outputExpectedNewIngestersInvoked)
        {
            // Arrange
            var commandDispatcherMock = Substitute.For<ICommandDispatcher>();

            var configMock = new ConfigBuiler()
                .With(
                    ingestedQueueName: "ingestedQ",
                    queueItemsPerRunningWorker: inputQueueItemsPerRunningWorker)
                .Build();
            
            var timeMock = Substitute.For<ITime>();
            timeMock.UtcNow.Returns(DateTime.Parse("2000-01-01 12:00"));
            
            var queueClientMock = new QueueClientMockBuilder()
                .WithRandomMessages(configMock.IngestedQueueName, inputNoOfMessages)
                .Build();

            var workerRecordStoreServiceMock = new WorkerRecordStoreServiceMockBuilder()
                .WithWorkerRecords(inputNoOfRunningIngesters, "mapper", timeMock.UtcNow)
                .Build();
            
            var workerManager = WorkerManagerFactory(
                commandDispatcher: commandDispatcherMock,
                config: configMock,
                queueClient: queueClientMock,
                workerRecordStoreService: workerRecordStoreServiceMock,
                time: timeMock);

            // Act
            await workerManager.InvokeAsync();

            // Assert
            await commandDispatcherMock.Received(outputExpectedNewIngestersInvoked)
                .DispatchAsync(Arg.Any<MapperCommand>());
        }
        
        [Test]
        public async Task Given_something_in_ingested_queue_and_a_mapper_has_stopped_responding__When_worker_manager_is_run__Then_a_mapper_is_invoked()
        {
            // Arrange
            var commandDispatcherMock = Substitute.For<ICommandDispatcher>();
        
            var configMock = new ConfigBuiler().Build();
                    
            var queueClientMock = new QueueClientMockBuilder()
                .WithRandomMessages(configMock.IngestedQueueName, 1)
                .Build();
        
            var timeMock = Substitute.For<ITime>();
            timeMock.UtcNow.Returns(DateTime.Parse("2018-02-27 12:00"));
        
            var workerRecordStoreServiceMock = new WorkerRecordStoreServiceMockBuilder()
                .WithWorkerRecord("mapper", DateTime.Parse("2018-02-27 11:59"))
                .Build();
                    
            var workerManager = WorkerManagerFactory(
                commandDispatcher: commandDispatcherMock,
                config: configMock,
                queueClient: queueClientMock,
                workerRecordStoreService: workerRecordStoreServiceMock,
                time: timeMock);
        
            // Act
            await workerManager.InvokeAsync();
        
            // Assert
            await commandDispatcherMock.Received().DispatchAsync(Arg.Any<MapperCommand>());
        }
        
        [Test]
        public async Task Given_there_are_no_workers_running_and_there_is_one_item_in_the_reduced_queue__When_worker_manager_is_run__Then_the_final_reduver_is_invoked()
        {
            // Arrange
            var commandDispatcherMock = Substitute.For<ICommandDispatcher>();

            var configMock = new ConfigBuiler()
                .With(reducedQueueName: "reducedQ")
                .Build();
            
            var queueClientMock = new QueueClientMockBuilder()
                .WithRandomMessages(configMock.ReducedQueueName, 1)
                .Build();

            var workerRecordStoreServiceMock = new WorkerRecordStoreServiceMockBuilder()
                .Build();
            
            var workerManager = WorkerManagerFactory(
                commandDispatcher: commandDispatcherMock,
                config: configMock,
                queueClient: queueClientMock,
                workerRecordStoreService: workerRecordStoreServiceMock);

            // Act
            await workerManager.InvokeAsync();

            // Assert
            await commandDispatcherMock.Received().DispatchAsync(Arg.Any<FinalReducerCommand>());
        }
        
        private WorkerManager WorkerManagerFactory(
            ICommandDispatcher commandDispatcher = null,
            IQueueClient queueClient = null,
            IConfig config = null,
            IWorkerRecordStoreService workerRecordStoreService = null,
            ITime time = null)
        {
            if (commandDispatcher == null)
                commandDispatcher = Substitute.For<ICommandDispatcher>();
            
            if (queueClient == null)
                queueClient = Substitute.For<IQueueClient>();
            
            if (config == null)
                config = Substitute.For<IConfig>();
            
            if (workerRecordStoreService == null)
                workerRecordStoreService = Substitute.For<IWorkerRecordStoreService>();

            if (time == null)
            {
                time = Substitute.For<ITime>();
            }
            
            var workerManager = new WorkerManager(commandDispatcher, queueClient, config, workerRecordStoreService, time);
            return workerManager;
        }
    }
}