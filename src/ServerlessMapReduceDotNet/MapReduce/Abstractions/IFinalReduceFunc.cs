using System.Collections.Generic;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Abstractions
{
    public interface IFinalReduceFunc
    {
        IReadOnlyCollection<string> FinalReduce(IKeyValuePair keyValuePair);
    }
}