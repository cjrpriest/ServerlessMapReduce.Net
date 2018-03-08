using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using NSubstitute;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Functions;
using ServerlessMapReduceDotNet.Services;
using ServerlessMapReduceDotNet.Tests.Builders;

namespace ServerlessMapReduceDotNet.Tests.UnitTests
{
    public class MapperTests
    {
        [Test]
        public async Task Given_one_message_on_the_ingested_queue__When_mapper_is_invoked__Then_has_termined_is_recorded()
        {
            var config = new ConfigBuiler().Build();
            
            var queueClientMock = new QueueClientMockBuilder()
                .WithRandomMessages(config.IngestedQueueName, 1)
                .Build();

            var objectStore = NullObjectStoreFactory();

            var workerRecordStoreServiceMock = Substitute.For<IWorkerRecordStoreService>();
            workerRecordStoreServiceMock.GenerateUniqueId().Returns("abc123");

            var mapper = MapperFactory(config: config, queueClient: queueClientMock, objectStore: objectStore, workerRecordStoreService: workerRecordStoreServiceMock);

            // Act
            await mapper.InvokeAsync();

            // Assert
            await workerRecordStoreServiceMock.Received().RecordHasTerminated(Arg.Is("mapper"), Arg.Is("abc123"));
        }
        
        [Test]
        public async Task Given_no_messages_on_the_ingested_queue__When_mapper_is_invoked__Then_has_termined_is_recorded()
        {
            var config = new ConfigBuiler().Build();
            
            var queueClientMock = new QueueClientMockBuilder()
                .WithRandomMessages(config.IngestedQueueName, 0)
                .Build();

            var objectStore = NullObjectStoreFactory();

            var workerRecordStoreServiceMock = Substitute.For<IWorkerRecordStoreService>();
            workerRecordStoreServiceMock.GenerateUniqueId().Returns("abc123");

            var mapper = MapperFactory(config: config, queueClient: queueClientMock, objectStore: objectStore, workerRecordStoreService: workerRecordStoreServiceMock);

            // Act
            await mapper.InvokeAsync();

            // Assert
            await workerRecordStoreServiceMock.Received().RecordHasTerminated(Arg.Is("mapper"), Arg.Is("abc123"));
        }

        private IObjectStore NullObjectStoreFactory()
        {
            var objectStore = Substitute.For<IObjectStore>();
            objectStore.RetrieveAsync(Arg.Any<string>())
                .Returns(ci => StreamHelper.NewEmptyStream());
            return objectStore;
        }
        
        private Mapper MapperFactory(
            IQueueClient queueClient = null,
            IObjectStore objectStore = null,
            IConfig config = null,
            IWorkerRecordStoreService workerRecordStoreService = null,
            ICommandDispatcher commandDispatcher = null)
        {
            queueClient = CheckParam(queueClient);
            objectStore = CheckParam(objectStore);
            config = CheckParam(config);
            workerRecordStoreService = CheckParam(workerRecordStoreService);
            commandDispatcher = CheckParam(commandDispatcher);

            var mapper = new Mapper(queueClient, objectStore, config, workerRecordStoreService, commandDispatcher);

            return mapper;
        }

        private T CheckParam<T>(T param) where T : class
        {
            return param ?? Substitute.For<T>();
        }
    }
}