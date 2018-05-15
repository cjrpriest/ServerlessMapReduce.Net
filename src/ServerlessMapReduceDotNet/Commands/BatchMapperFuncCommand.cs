using System.Collections.Generic;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.Queue;

namespace ServerlessMapReduceDotNet.Commands
{
    public class BatchMapperFuncCommand : ICommand
    {
        public IReadOnlyCollection<string> Lines { get; set; }
        public QueueMessage ContextQueueMessage { get; set; }
    }
}