using System.Collections.Generic;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Commands.Reduce;
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

        public IReadOnlyCollection<string> FinalReduce2(CompressedMostAccidentProneData compressedMostAccidentProneData)
        {
            throw new System.NotImplementedException();
        }
    }
}