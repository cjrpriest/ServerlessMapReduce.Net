using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Helpers;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Queue.InMemory
{
    public class InMemoryQueueClient : IQueueClient
    {
        private readonly ITime _time;
        private readonly InMemoryInternalQueueCollection _queues = new InMemoryInternalQueueCollection();
        private readonly object _queueCollectionLock = new object();

        private int _messageSequenceNumber = 0;
        
        public InMemoryQueueClient(ITime time)
        {
            _time = time;
        }

        public async Task Enqueue(string queueName, string message)
        {
            lock (_queueCollectionLock)
            {
                Console.WriteLine($"Enqueing message {message.TopAndTail(1000)} to queue {queueName}");
                EnsureQueuePresent(queueName);
                var newMessageId = Guid.NewGuid().ToString();
                _queues[queueName].TryAdd(newMessageId,
                    new InMemoryInternalQueueMessage
                    {
                        SequenceNumber = Interlocked.Increment(ref _messageSequenceNumber),
                        MessageId = newMessageId, 
                        Message = message
                    });
            }
        }

        public async Task<IList<QueueMessage>> Dequeue(string queueName, int maxMessagesToDequeue = 1)
        {
            var messages = new List<QueueMessage>();
            lock (_queueCollectionLock)
            {
                if (!_queues.ContainsKey(queueName)) return messages;
                
                HousekeepQueue(_queues[queueName]);
                for (int i = 0; i < maxMessagesToDequeue; i++)
                {
                    var success = Dequeue(_queues[queueName], out var message);
                    if (!success) break;
                    
                    Console.WriteLine($"Deqeuing message {message.Message.TopAndTail(1000)} from queue {queueName}");
                    messages.Add(new QueueMessage
                    {
                        MessageId = message.MessageId,
                        Message = message.Message
                    });
                }
            }
            return messages;
        }

        private void HousekeepQueue(ConcurrentDictionary<string, InMemoryInternalQueueMessage> internalQueueMessages)
        {
            var now = _time.UtcNow;
            var expiredHiddenMessages =
                internalQueueMessages
                    .Where(x => x.Value.IsHidden && (x.Value.TimeHidden + x.Value.VisibilityPeriod < now));
            foreach (var expiredHiddenMessage in expiredHiddenMessages)
            {
                expiredHiddenMessage.Value.IsHidden = false;
            }
        }

        public async Task MessageProcessed(string queueName, string messageId)
        {
            lock (_queueCollectionLock)
            {
                if (!_queues[queueName].ContainsKey(messageId))
                    throw new ApplicationException($"Message with Id {messageId} could not be found");
                _queues[queueName].Remove(messageId, out var _);
            }
        }

        public async Task ReturnMessageToQueue(string queueName, string messageId)
        {
            lock (_queueCollectionLock)
            {
                if (!_queues[queueName].ContainsKey(messageId))
                    throw new ApplicationException($"Message with Id {messageId} could not be found");

                var internalQueueMessage = _queues[queueName][messageId];
                internalQueueMessage.VisibilityPeriod = TimeSpan.Zero;
            }
        }

        public Task<int> MessageCount(string queuename)
        {
            lock (_queueCollectionLock)
            {
                if (_queues.ContainsKey(queuename))
                {
                    return Task.FromResult(_queues[queuename].Count);
                }

                return Task.FromResult(0);
            }
        }
        
        private void EnsureQueuePresent(string queueName)
        {
            if (_queues.ContainsKey(queueName)) return;

            InitialiseQueueIfNotPresent(queueName);
        }

        private void InitialiseQueueIfNotPresent(string queueName)
        {
            if (!_queues.ContainsKey(queueName))
            {
                _queues.Add(queueName, new ConcurrentDictionary<string, InMemoryInternalQueueMessage>());
            }
        }

        private bool Dequeue(ConcurrentDictionary<string, InMemoryInternalQueueMessage> queue, out InMemoryInternalQueueMessage dequeuedMessage)
        {
            if (queue.Any(x => !x.Value.IsHidden))
            {
                dequeuedMessage = queue
                    .Where(x => !x.Value.IsHidden)
                    .OrderBy(x => x.Value.SequenceNumber)
                    .First()
                    .Value;
                dequeuedMessage.IsHidden = true;
                dequeuedMessage.TimeHidden = _time.UtcNow;
                dequeuedMessage.VisibilityPeriod = TimeSpan.FromSeconds(30);
                return true;
            }

            dequeuedMessage = null;
            return false;
        }
    }
}