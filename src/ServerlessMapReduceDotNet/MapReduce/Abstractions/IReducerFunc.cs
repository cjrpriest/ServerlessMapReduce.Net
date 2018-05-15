using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Abstractions
{
    public interface IReducerFunc
    {
        KeyValuePairCollection Reduce(KeyValuePairCollection inputKeyValuePairs);
    }
}