using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands
{
    public class MapperFuncCommand : ICommand<KeyValuePairCollection>
    {
        public string Line { get; set; }
    }
}