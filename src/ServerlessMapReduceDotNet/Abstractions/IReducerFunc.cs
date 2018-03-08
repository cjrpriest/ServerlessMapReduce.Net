using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Abstractions
{
    public interface IReducerFunc
    {
        KeyValuePairCollection Reduce(KeyValuePairCollection inputKeyValuePairs);
    }
}