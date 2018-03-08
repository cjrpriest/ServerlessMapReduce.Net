using System.Collections.Generic;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Abstractions
{
    public interface IFinalReduceFunc
    {
        IReadOnlyCollection<string> FinalReduce(IKeyValuePair keyValuePair);
    }
}