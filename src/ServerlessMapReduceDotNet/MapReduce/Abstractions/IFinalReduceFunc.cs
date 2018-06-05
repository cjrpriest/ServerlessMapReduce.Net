using System.Collections.Generic;
using ServerlessMapReduceDotNet.MapReduce.Commands.Reduce;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Abstractions
{
    public interface IFinalReduceFunc
    {
        IReadOnlyCollection<string> FinalReduce(IKeyValuePair keyValuePair);
        IReadOnlyCollection<string> FinalReduce2(CompressedMostAccidentProneData compressedMostAccidentProneData);
    }
}