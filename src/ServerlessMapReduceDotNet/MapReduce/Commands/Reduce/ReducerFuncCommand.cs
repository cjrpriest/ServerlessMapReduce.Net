using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands.Reduce
{
    public class ReducerFuncCommand : ICommand<KeyValuePairCollection>
    {
        public KeyValuePairCollection InputKeyValuePairs { get; set; }
    }
}