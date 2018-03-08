using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Abstractions;
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

            var objectStore = Substitute.For<IObjectStore>();
            objectStore.RetrieveAsync(Arg.Any<string>())
                .Returns(ci => StreamHelper.NewEmptyStream());

            var ingester = FinalReducerFactory(config: config, queueClient: queueClientMock, objectStore: objectStore);

            // Act
            await ingester.InvokeAsync();

            // Assert
            await queueClientMock.Received(1).MessageProcessed(Arg.Is(config.ReducedQueueName), Arg.Any<string>());
        }

        private FinalReducer FinalReducerFactory(
            IQueueClient queueClient = null,
            IObjectStore objectStore = null,
            IConfig config = null,
            IWorkerRecordStoreService workerRecordStoreService = null)
        {
            queueClient = CheckParam(queueClient);
            objectStore = CheckParam(objectStore);
            config = CheckParam(config);
            workerRecordStoreService = CheckParam(workerRecordStoreService);

            var finalReducer = new FinalReducer(objectStore, queueClient, config, workerRecordStoreService);

            return finalReducer;
        }

        private T CheckParam<T>(T param) where T : class
        {
            return param ?? Substitute.For<T>();
        }
    }
}