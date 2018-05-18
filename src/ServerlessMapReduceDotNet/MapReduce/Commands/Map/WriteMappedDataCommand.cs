using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands.Map
{
    public class WriteMappedDataCommand : ICommand
    {
        public KeyValuePairCollection ResultOfMap { get; set; }
        public QueueMessage ContextQueueMessage { get; set; }
    }
}