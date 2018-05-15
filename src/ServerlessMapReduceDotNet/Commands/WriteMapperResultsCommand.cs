using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Commands
{
    public class WriteMapperResultsCommand : ICommand
    {
        public KeyValuePairCollection ResultOfMap { get; set; }
        public string IngestedDataObjectName { get; set; }
        public string IngestedQueueMessageId { get; set; }
    }
}