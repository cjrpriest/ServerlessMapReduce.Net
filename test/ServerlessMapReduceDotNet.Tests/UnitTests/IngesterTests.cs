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
    public class IngesterTests
    {
        [Test]
        public async Task Given_a_message_on_raw_queue__When_reducer_is_invoked__Then_the_message_is_marked_processed()
        {
            // Arrange
            var config = new ConfigBuiler().Build();
            
            var queueClientMock = new QueueClientMockBuilder()
                .WithRandomMessages(config.RawDataQueueName, 1)
                .Build();

            var commandDispatcher = Substitute.For<ICommandDispatcher>();
            commandDispatcher
                .DispatchAsync(Arg.Any<RetrieveObjectCommand>())
                .ReturnsCommandResult(StreamHelper.NewEmptyStream());

            var ingester = IngestorFactory(config: config, queueClient: queueClientMock, commandDispatcher: commandDispatcher);

            // Act
            await ingester.InvokeAsync();

            // Assert
            await queueClientMock.Received(1).MessageProcessed(Arg.Is(config.RawDataQueueName), Arg.Any<string>());
        }

        private Ingester IngestorFactory(
            IQueueClient queueClient = null,
            IConfig config = null,
            IWorkerRecordStoreService workerRecordStoreService = null,
            ICommandDispatcher commandDispatcher = null)
        {
            queueClient = CheckParam(queueClient);
            config = CheckParam(config);
            workerRecordStoreService = CheckParam(workerRecordStoreService);
            commandDispatcher = CheckParam(commandDispatcher);

            var ingester = new Ingester(queueClient, config, workerRecordStoreService, commandDispatcher);

            return ingester;
        }

        private T CheckParam<T>(T param) where T : class
        {
            return param ?? Substitute.For<T>();
        }
    }
}