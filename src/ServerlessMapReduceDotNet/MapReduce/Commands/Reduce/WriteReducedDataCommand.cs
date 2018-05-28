using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands.Reduce
{
    public class WriteReducedDataCommand : ICommand
    {
        public KeyValuePairCollection ReducedData { get; set; }
        public string ProcessedMessageIdsHash { get; set; }
    }
}