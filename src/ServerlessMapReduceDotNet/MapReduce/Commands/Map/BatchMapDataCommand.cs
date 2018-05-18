using System.Collections.Generic;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands.Map
{
    public class BatchMapDataCommand : ICommand
    {
        public IReadOnlyCollection<string> Lines { get; set; }
        public QueueMessage ContextQueueMessage { get; set; }
    }
}