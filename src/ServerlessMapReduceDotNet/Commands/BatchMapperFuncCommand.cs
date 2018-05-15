using System.Collections.Generic;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Commands
{
    public class BatchMapperFuncCommand : ICommand
    {
        public IReadOnlyCollection<string> Lines { get; set; }
        public string IngestedDataObjectName { get; set; }
        public string IngestedQueueMessageId { get; set; }
    }
}