using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Abstractions
{
    public interface IMapperFunc
    {
        KeyValuePairCollection Map(string line);
    }
}