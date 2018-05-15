using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using NSubstitute;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.MapReduce.FireAndForgetFunctions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using ServerlessMapReduceDotNet.Services;
using ServerlessMapReduceDotNet.Tests.Builders;
using ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock;

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

            var commandDispatcher = Substitute.For<ICommandDispatcher>();
                commandDispatcher.DispatchAsync(Arg.Any<RetrieveObjectCommand>())
                    .ReturnsCommandResult(StreamHelper.NewEmptyStream());

            var workerRecordStoreServiceMock = Substitute.For<IWorkerRecordStoreService>();
            workerRecordStoreServiceMock.GenerateUniqueId().Returns("abc123");

            var mapper = MapperFactory(config: config, queueClient: queueClientMock, commandDispatcher: commandDispatcher, workerRecordStoreService: workerRecordStoreServiceMock);

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

            var commandDispatcher = Substitute.For<ICommandDispatcher>();
            commandDispatcher.DispatchAsync(Arg.Any<RetrieveObjectCommand>())
                .ReturnsCommandResult(StreamHelper.NewEmptyStream());

            var workerRecordStoreServiceMock = Substitute.For<IWorkerRecordStoreService>();
            workerRecordStoreServiceMock.GenerateUniqueId().Returns("abc123");

            var mapper = MapperFactory(config: config, queueClient: queueClientMock, commandDispatcher: commandDispatcher, workerRecordStoreService: workerRecordStoreServiceMock);

            // Act
            await mapper.InvokeAsync();

            // Assert
            await workerRecordStoreServiceMock.Received().RecordHasTerminated(Arg.Is("mapper"), Arg.Is("abc123"));
        }
        
        private Mapper MapperFactory(
            IQueueClient queueClient = null,
            IConfig config = null,
            IWorkerRecordStoreService workerRecordStoreService = null,
            ICommandDispatcher commandDispatcher = null)
        {
            queueClient = CheckParam(queueClient);
            config = CheckParam(config);
            workerRecordStoreService = CheckParam(workerRecordStoreService);
            commandDispatcher = CheckParam(commandDispatcher);

            var mapper = new Mapper(queueClient, config, workerRecordStoreService, commandDispatcher);

            return mapper;
        }

        private T CheckParam<T>(T param) where T : class
        {
            return param ?? Substitute.For<T>();
        }
    }
}