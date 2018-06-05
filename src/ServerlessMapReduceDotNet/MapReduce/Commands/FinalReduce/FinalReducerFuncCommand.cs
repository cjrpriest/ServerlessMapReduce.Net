using System.Collections.Generic;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Commands.Reduce;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands.FinalReduce
{
    public class FinalReducerFuncCommand : ICommand<IReadOnlyCollection<string>>
    {
        public IKeyValuePair KeyValuePair { get; set; }
        public CompressedMostAccidentProneData CompressedMostAccidentProneData { get; set; }
    }
}