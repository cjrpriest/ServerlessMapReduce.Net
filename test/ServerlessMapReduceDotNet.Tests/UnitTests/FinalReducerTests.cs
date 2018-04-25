using System.IO;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using AzureFromTheTrenches.Commanding.Abstractions.Model;
using NSubstitute;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.Functions;
using ServerlessMapReduceDotNet.Services;
using ServerlessMapReduceDotNet.Tests.Builders;

namespace ServerlessMapReduceDotNet.Tests.UnitTests
{
    public class FinalReducerTests
    {
        [Test]
        public async Task Given_a_message_on_reduced_queue__When_final_reducer_is_invoked__Then_the_message_is_marked_processed()
        {
            // Arrange
            var config = new ConfigBuiler().Build();
            
            var queueClientMock = new QueueClientMockBuilder()
                .WithRandomMessages(config.ReducedQueueName, 1)
                .Build();

            var commandDispatcher = Substitute.For<ICommandDispatcher>();
            commandDispatcher.DispatchAsync(Arg.Any<RetrieveObjectCommand>())
                .Returns(new CommandResult<Stream>(StreamHelper.NewEmptyStream(), false));

            var ingester = FinalReducerFactory(config: config, queueClient: queueClientMock, commandDispatcher: commandDispatcher);

            // Act
            await ingester.InvokeAsync();

            // Assert
            await queueClientMock.Received(1).MessageProcessed(Arg.Is(config.ReducedQueueName), Arg.Any<string>());
        }

        private FinalReducer FinalReducerFactory(
            IQueueClient queueClient = null,
            IConfig config = null,
            IWorkerRecordStoreService workerRecordStoreService = null,
            ICommandDispatcher commandDispatcher = null)
        {
            queueClient = CheckParam(queueClient);
            config = CheckParam(config);
            workerRecordStoreService = CheckParam(workerRecordStoreService);
            commandDispatcher = CheckParam(commandDispatcher);

            var finalReducer = new FinalReducer(queueClient, config, workerRecordStoreService, commandDispatcher);

            return finalReducer;
        }

        private T CheckParam<T>(T param) where T : class
        {
            return param ?? Substitute.For<T>();
        }
    }
}