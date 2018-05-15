using System.Collections.Generic;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Functions.MakeAccidentCount
{
    class MakeAccidentCountFinalReducer : IFinalReduceFunc
    {
        public IReadOnlyCollection<string> FinalReduce(IKeyValuePair keyValuePair)
        {
            var countKvp = (CountKvp) keyValuePair;
            return new[] {$"{countKvp.Key},{countKvp.Value}"};
        }
    }
}