using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Abstractions
{
    public interface IMapperFunc
    {
        KeyValuePairCollection Map(string line);
    }
}