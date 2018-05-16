using System.Collections.Generic;
using System.Threading.Tasks;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions
{
    public interface IQueueClient
    {
        Task Enqueue(string queueName, string message);
        Task<IList<QueueMessage>> Dequeue(string queueName, int maxMessagesToDequeue = 1);
        Task MessageProcessed(string queueName, string messageId);
        Task ReturnMessageToQueue(string queueName, string messageId);
        Task<int> MessageCount(string queuename);
    }
} 