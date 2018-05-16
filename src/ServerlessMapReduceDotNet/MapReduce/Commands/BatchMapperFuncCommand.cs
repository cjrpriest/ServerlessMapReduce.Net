using System.Collections.Generic;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands
{
    public class BatchMapperFuncCommand : ICommand
    {
        public IReadOnlyCollection<string> Lines { get; set; }
        public QueueMessage ContextQueueMessage { get; set; }
    }
}