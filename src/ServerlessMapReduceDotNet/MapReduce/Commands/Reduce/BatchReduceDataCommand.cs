using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands.Reduce
{
    public class BatchReduceDataCommand : ICommand
    {
        public KeyValuePairCollection InputKeyValuePairs { get; set; }
        public string ProcessedMessageIdsHash { get; set; }
    }
}