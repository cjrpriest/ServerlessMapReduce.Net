using System.Collections.Generic;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Commands
{
    public class FinalReducerFuncCommand : ICommand<IReadOnlyCollection<string>>
    {
        public IKeyValuePair KeyValuePair { get; set; }
    }
}