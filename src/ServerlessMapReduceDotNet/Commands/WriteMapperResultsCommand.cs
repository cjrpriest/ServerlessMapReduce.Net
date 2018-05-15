using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.Queue;

namespace ServerlessMapReduceDotNet.Commands
{
    public class WriteMapperResultsCommand : ICommand
    {
        public KeyValuePairCollection ResultOfMap { get; set; }
        public QueueMessage ContextQueueMessage { get; set; }
    }
}