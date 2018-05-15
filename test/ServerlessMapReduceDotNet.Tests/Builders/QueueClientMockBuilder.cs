using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using ServerlessMapReduceDotNet.Queue;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.Tests.Builders
{
    public class QueueClientMockBuilder
    {
        private readonly IQueueClient _queueClientMock = Substitute.For<IQueueClient>();
        
        private Dictionary<string, Queue<QueueMessage>> _queues = new Dictionary<string, Queue<QueueMessage>>();

        public QueueClientMockBuilder WithMessage(string queueName, string message)
        {
            return WithQueueItems(queueName, new[] { new QueueMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Message = message
            }});
        }
        
        public QueueClientMockBuilder WithQueueItems(string queueName, QueueMessage message)
        {
            return WithQueueItems(queueName, new[] {message});
        }
        
        public QueueClientMockBuilder WithQueueItems(string queueName, IReadOnlyCollection<QueueMessage> messages)
        {
            if (!_queues.ContainsKey(queueName))
                _queues.Add(queueName, new Queue<QueueMessage>());

            foreach (var message in messages)
            {
                _queues[queueName].Enqueue(message);                
            }

            _queueClientMock.MessageCount(Arg.Is(queueName))
                .Returns(_queues[queueName].Count);

            _queueClientMock.Dequeue(Arg.Is(queueName), Arg.Any<int>()).Returns((callInfo) =>
            {
                var noOfMessages = (int)callInfo[1];
                var qName = (string) callInfo[0];
                var messagesToReturn = new List<QueueMessage>();
                for (int i = 0; i < noOfMessages; i++)
                {
                    if (_queues[qName].Count == 0) continue;
                    messagesToReturn.Add(_queues[qName].Dequeue());    
                }
                return messagesToReturn;
            });

            return this;
        }

        public QueueClientMockBuilder WithRandomMessages(string queueName, int quantityOfMessages)
        {
            var messages = new List<QueueMessage>();
            
            for (int i = 0; i < quantityOfMessages; i++)
            {
                messages.Add(new QueueMessage
                {
                    Message = RandomString(10),
                    MessageId = Guid.NewGuid().ToString()
                });
            }

            return WithQueueItems(queueName, messages);
        }
        
        public IQueueClient Build()
        {
            return _queueClientMock;
        }
        
        private static readonly Random Random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}