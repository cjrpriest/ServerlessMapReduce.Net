using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands.Map
{
    public class MapDataCommand : ICommand<KeyValuePairCollection>
    {
        public string Line { get; set; }
    }
}