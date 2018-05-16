using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Queue.InMemory;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Queue.InMemory;
using Shouldly;

namespace ServerlessMapReduceDotNet.Tests.UnitTests
{
    public class InternalQueueClientTests
    {
        [Test]
        public async Task Given_message_1_is_on_the_queue__When_an_item_is_dequeued__Then_message_1_is_retrieved()
        {
            // Arrange
            var queue = InternalQueueClientFactory();
            await queue.Enqueue("queue1", "1");

            // Act
            var messages = await queue.Dequeue("queue1");

            // Assert
            messages.Count.ShouldBe(1);
            messages.First().Message.ShouldBe("1");
        }
        
        [Test]
        public async Task Given_message_1_then_2_is_on_the_queue__When_the_2nd_item_is_dequeued__Then_message_2_is_retrieved()
        {
            // Arrange
            var queue = InternalQueueClientFactory();
            await queue.Enqueue("queue1", "1");
            await queue.Enqueue("queue1", "2");
            await queue.Dequeue("queue1");

            // Act
            var messages = await queue.Dequeue("queue1");

            // Assert
            messages.Count.ShouldBe(1);
            messages.First().Message.ShouldBe("2");
        }
        
        [Test]
        public async Task Given_no_messages_on_the_queue__When_an_item_is_dequeued__Then_null_is_retrieved()
        {
            // Arrange
            var queue = InternalQueueClientFactory();

            // Act
            var messages = await queue.Dequeue("queue1");

            // Assert
            messages.Count.ShouldBe(0);
        }
        
        [Test]
        public async Task Given_message_1_is_on_the_queue1__When_an_item_is_dequeued_from_queue2__Then_null_is_retrieved()
        {
            // Arrange
            var queue = InternalQueueClientFactory();
            await queue.Enqueue("queue1", "1");

            // Act
            var messages = await queue.Dequeue("queue2");

            // Assert
            messages.Count.ShouldBe(0);
        }
        
        [Test]
        public async Task Given_message_1_is_on_queue1_and_message_2_is_on_queue2__When_an_item_is_dequeued_from_queue2__Then_message_2_is_retrieved()
        {
            // Arrange
            var queue = InternalQueueClientFactory();
            await queue.Enqueue("queue1", "1");
            await queue.Enqueue("queue2", "2");

            // Act
            var messages = await queue.Dequeue("queue2");

            // Assert
            messages.Count.ShouldBe(1);
            messages.First().Message.ShouldBe("2");
        }
        
        [Test]
        public async Task Given_10_messages_on_queue__When_5_items_are_dequeued__Then_first_5_messages_are_retrieved()
        {
            // Arrange
            var queue = InternalQueueClientFactory();
            for (int i = 1; i <= 10; i++)
            {
                await queue.Enqueue("queue1", i.ToString());
            }

            // Act
            var messages = await queue.Dequeue("queue1", 5);

            // Assert
            messages.Count.ShouldBe(5);
            messages[0].Message.ShouldBe("1");
            messages[1].Message.ShouldBe("2");
            messages[2].Message.ShouldBe("3");
            messages[3].Message.ShouldBe("4");
            messages[4].Message.ShouldBe("5");
        }
        
        [Test]
        public async Task Given_a_message_1_is_dequeued_and_returned_to_the_queue__When_a_message_is_dequeued__Then_message_is_1()
        {
            // Arrange
            var queueName = "queue1";
            var queue = InternalQueueClientFactory();
            
            await queue.Enqueue(queueName, "1");
            
            var queueMessages = await queue.Dequeue(queueName);
            await queue.ReturnMessageToQueue(queueName, queueMessages.First().MessageId);

            // Act
            var messages = await queue.Dequeue(queueName);

            // Assert
            messages.Count.ShouldBe(1);
            messages.First().Message.ShouldBe("1");
        }
        
        [Test]
        public async Task Given_a_message_1_is_dequeued_and_marked_processed__When_a_message_is_dequeued__Then_the_next_message_is_retrieved()
        {
            // Arrange
            var queueName = "queue1";
            var queue = InternalQueueClientFactory();
            
            await queue.Enqueue(queueName, "1");
            await queue.Enqueue(queueName, "2");
            
            var queueMessages = await queue.Dequeue(queueName);
            await queue.MessageProcessed(queueName, queueMessages.First().MessageId);

            // Act
            var messages = await queue.Dequeue(queueName);

            // Assert
            messages.Count.ShouldBe(1);
            messages.First().Message.ShouldBe("2");
        }
        
        [Test]
        public async Task Given_two_messages_are_dequeued_and_first_is_returned__When_a_message_is_dequeued__Then_the_first_message_is_retrieved()
        {
            // Arrange
            var queueName = "queue1";
            var queue = InternalQueueClientFactory();
            
            await queue.Enqueue(queueName, "1");
            await queue.Enqueue(queueName, "2");
            
            var queueMessages = await queue.Dequeue(queueName);
            await queue.Dequeue(queueName);
            
            await queue.ReturnMessageToQueue(queueName, queueMessages.First().MessageId);

            // Act
            var messages = await queue.Dequeue(queueName);

            // Assert
            messages.Count.ShouldBe(1);
            messages.First().Message.ShouldBe("1");
        }

        private InMemoryQueueClient InternalQueueClientFactory()
        {
            return new InMemoryQueueClient(new Time());
        }
    }
}