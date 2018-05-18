using System.Collections.Generic;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands.FinalReduce
{
    public class FinalReducerFuncCommand : ICommand<IReadOnlyCollection<string>>
    {
        public IKeyValuePair KeyValuePair { get; set; }
    }
}