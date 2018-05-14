using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Commands
{
    public class ReducerFuncCommand : ICommand<KeyValuePairCollection>
    {
        public KeyValuePairCollection InputKeyValuePairs { get; set; }
    }
}