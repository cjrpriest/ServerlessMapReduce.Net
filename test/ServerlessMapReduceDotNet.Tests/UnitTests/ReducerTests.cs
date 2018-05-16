using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using NSubstitute;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.MapReduce.FireAndForgetFunctions;
using ServerlessMapReduceDotNet.MapReduce.Functions.MostAccidentProne;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Helpers;
using ServerlessMapReduceDotNet.Tests.Builders;
using ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock;

namespace ServerlessMapReduceDotNet.Tests.UnitTests
{
    public class ReducerTests
    {
        [Test]
        public async Task Given_0_messages_on_mapped_1_on_reduced_queues__When_reducer_is_invoked__Then_no_work_is_done()
        {
            // Arrange
            var config = new ConfigBuiler().Build();
            
            var queueClientMock = new QueueClientMockBuilder()
                .WithRandomMessages(config.MappedQueueName, 0)
                .WithRandomMessages(config.ReducedQueueName, 1)
                .Build();

            var commandDispatcher = Substitute.For<ICommandDispatcher>();
            commandDispatcher.DispatchAsync(Arg.Any<RetrieveObjectCommand>())
                .ReturnsCommandResult(StreamHelper.NewEmptyStream());

            var reducer = ReducerFactory(config: config, queueClient: queueClientMock, commandDispatcher: commandDispatcher);

            // Act
            await reducer.InvokeAsync();

            // Assert
            await commandDispatcher.DidNotReceiveWithAnyArgs().DispatchAsync(null);
        }
        
        [Test]
        public async Task Given_0_messages_on_mapped_0_on_reduced_queues__When_reducer_is_invoked__Then_no_work_is_done()
        {
            // Arrange
            var config = new ConfigBuiler().Build();
            
            var queueClientMock = new QueueClientMockBuilder()
                .WithRandomMessages(config.MappedQueueName, 0)
                .WithRandomMessages(config.ReducedQueueName, 0)
                .Build();

            var commandDispatcher = Substitute.For<ICommandDispatcher>();
            commandDispatcher.DispatchAsync(Arg.Any<RetrieveObjectCommand>())
                .ReturnsCommandResult(StreamHelper.NewEmptyStream);

            var reducer = ReducerFactory(config: config, queueClient: queueClientMock, commandDispatcher: commandDispatcher);

            // Act
            await reducer.InvokeAsync();

            // Assert
            await commandDispatcher.DidNotReceiveWithAnyArgs().DispatchAsync(null);
        }
        
        [Test]
        public async Task Given_a_message_on_mapped_and_reduced_queues__When_reducer_is_invoked__Then_both_messages_are_marked_processed()
        {
            // Arrange
            var config = new ConfigBuiler().Build();
            
            var queueClientMock = new QueueClientMockBuilder()
                .WithRandomMessages(config.MappedQueueName, 1)
                .WithRandomMessages(config.ReducedQueueName, 1)
                .Build();

            var commandDispatcher = Substitute.For<ICommandDispatcher>();
            commandDispatcher.DispatchAsync(Arg.Any<RetrieveObjectCommand>())
                .ReturnsCommandResult(StreamHelper.NewEmptyStream);

            var reducer = ReducerFactory(config: config, queueClient: queueClientMock, commandDispatcher: commandDispatcher);

            // Act
            await reducer.InvokeAsync();

            // Assert
            await queueClientMock.Received(1).MessageProcessed(Arg.Is(config.MappedQueueName), Arg.Any<string>());
            await queueClientMock.Received(1).MessageProcessed(Arg.Is(config.ReducedQueueName), Arg.Any<string>());
        }
        
        [Test]
        public async Task Given_a_message_on_mapped_queue__When_reducer_is_invoked__Then_mapped_object_is_retrieved()
        {
            // Arrange
            var config = new ConfigBuiler().Build();
            
            var queueClientMock = new QueueClientMockBuilder()
                .WithMessage(config.MappedQueueName, $"{config.MappedFolder}/mappedobject1")
                .Build();

            var commandDispatcher = Substitute.For<ICommandDispatcher>();
            commandDispatcher.DispatchAsync(Arg.Any<RetrieveObjectCommand>())
                .ReturnsCommandResult(StreamHelper.NewEmptyStream);

            var reducer = ReducerFactory(config: config, queueClient: queueClientMock, commandDispatcher: commandDispatcher);

            // Act
            await reducer.InvokeAsync();

            // Assert
            await commandDispatcher.Received()
                .DispatchAsync(Arg.Is<RetrieveObjectCommand>(x =>
                    x.Key == $"{config.MappedFolder}/mappedobject1"));
        }

        [Test]
        public async Task Given_messages_on_reduced_queue__When_reducer_is_invoked__Then_reduced_objects_are_retrieved()
        {
            // Arrange
            var config = new ConfigBuiler().Build();

            var queueClientMock = new QueueClientMockBuilder()
                .WithMessage(config.ReducedQueueName, $"{config.ReducedFolder}/reducedobject1")
                .WithMessage(config.ReducedQueueName, $"{config.ReducedFolder}/reducedobject2")
                .Build();

            var commandDispatcher = Substitute.For<ICommandDispatcher>();
            commandDispatcher.DispatchAsync(Arg.Any<RetrieveObjectCommand>())
                .ReturnsCommandResult(StreamHelper.NewEmptyStream);

            var reducer = ReducerFactory(config: config, queueClient: queueClientMock,
                commandDispatcher: commandDispatcher);

            // Act
            await reducer.InvokeAsync();

            // Assert
            await commandDispatcher.Received()
                .DispatchAsync(Arg.Is<RetrieveObjectCommand>(x =>
                    x.Key == $"{config.ReducedFolder}/reducedobject1"));
            await commandDispatcher.Received()
                .DispatchAsync(Arg.Is<RetrieveObjectCommand>(x =>
                    x.Key == $"{config.ReducedFolder}/reducedobject2"));
        }

        private Reducer ReducerFactory(
            IQueueClient queueClient = null,
            IConfig config = null,
            IWorkerRecordStoreService workerRecordStoreService = null,
            ICommandDispatcher commandDispatcher = null)
        {
            queueClient = CheckParam(queueClient);
            config = CheckParam(config);
            workerRecordStoreService = CheckParam(workerRecordStoreService);

            commandDispatcher = CheckParam(commandDispatcher);
            commandDispatcher.RegisterReducerFunc(new MostAccidentProneReducer());

            var redcuer = new Reducer(queueClient, config, workerRecordStoreService, commandDispatcher);

            return redcuer;
        }

        private T CheckParam<T>(T param) where T : class
        {
            return param ?? Substitute.For<T>();
        }
    }
}