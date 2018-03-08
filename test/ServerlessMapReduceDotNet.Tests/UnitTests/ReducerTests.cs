using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Functions;
using ServerlessMapReduceDotNet.Services;
using ServerlessMapReduceDotNet.Tests.Builders;

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

            var objectStoreMock = Substitute.For<IObjectStore>();
            objectStoreMock.RetrieveAsync(Arg.Any<string>())
                .Returns(ci => StreamHelper.NewEmptyStream());

            var reducer = ReducerFactory(config: config, queueClient: queueClientMock, objectStore: objectStoreMock);

            // Act
            await reducer.InvokeAsync();

            // Assert
            await objectStoreMock.DidNotReceiveWithAnyArgs().RetrieveAsync(null);
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

            var objectStoreMock = Substitute.For<IObjectStore>();
            objectStoreMock.RetrieveAsync(Arg.Any<string>())
                .Returns(ci => StreamHelper.NewEmptyStream());

            var reducer = ReducerFactory(config: config, queueClient: queueClientMock, objectStore: objectStoreMock);

            // Act
            await reducer.InvokeAsync();

            // Assert
            await objectStoreMock.DidNotReceiveWithAnyArgs().RetrieveAsync(null);
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

            var objectStore = Substitute.For<IObjectStore>();
            objectStore.RetrieveAsync(Arg.Any<string>())
                .Returns(ci => StreamHelper.NewEmptyStream());

            var reducer = ReducerFactory(config: config, queueClient: queueClientMock, objectStore: objectStore);

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

            var objectStoreMock = Substitute.For<IObjectStore>();
            objectStoreMock.RetrieveAsync(Arg.Any<string>())
                .Returns(ci => StreamHelper.NewEmptyStream());

            var reducer = ReducerFactory(config: config, queueClient: queueClientMock, objectStore: objectStoreMock);

            // Act
            await reducer.InvokeAsync();

            // Assert
            await objectStoreMock.Received().RetrieveAsync($"{config.MappedFolder}/mappedobject1");
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

            var objectStoreMock = Substitute.For<IObjectStore>();
            objectStoreMock.RetrieveAsync(Arg.Any<string>())
                .Returns(ci => StreamHelper.NewEmptyStream());

            var reducer = ReducerFactory(config: config, queueClient: queueClientMock, objectStore: objectStoreMock);

            // Act
            await reducer.InvokeAsync();

            // Assert
            await objectStoreMock.Received().RetrieveAsync($"{config.ReducedFolder}/reducedobject1");
            await objectStoreMock.Received().RetrieveAsync($"{config.ReducedFolder}/reducedobject2");
        }

        private Reducer ReducerFactory(
            IQueueClient queueClient = null,
            IObjectStore objectStore = null,
            IConfig config = null,
            IWorkerRecordStoreService workerRecordStoreService = null)
        {
            queueClient = CheckParam(queueClient);
            objectStore = CheckParam(objectStore);
            config = CheckParam(config);
            workerRecordStoreService = CheckParam(workerRecordStoreService);

            var redcuer = new Reducer(queueClient, objectStore, config, workerRecordStoreService);

            return redcuer;
        }

        private T CheckParam<T>(T param) where T : class
        {
            return param ?? Substitute.For<T>();
        }
    }
}