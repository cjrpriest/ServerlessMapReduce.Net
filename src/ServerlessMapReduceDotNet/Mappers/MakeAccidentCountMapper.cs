using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Mappers
{
    public class MakeAccidentCountMapper : IMapperFunc
    {
        public KeyValuePairCollection Map(string line)
        {
            var strings = line.Split(',');
            var count = new CountKvp
            {
                Key = strings[22], // make is 23rd column
                Value = 1
            };
            return new KeyValuePairCollection {count};
        }
    }
}