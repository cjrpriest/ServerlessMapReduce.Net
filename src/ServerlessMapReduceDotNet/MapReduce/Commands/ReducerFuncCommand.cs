using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands
{
    public class ReducerFuncCommand : ICommand<KeyValuePairCollection>
    {
        public KeyValuePairCollection InputKeyValuePairs { get; set; }
    }
}