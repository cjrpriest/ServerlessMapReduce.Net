using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Functions.MakeAccidentCount
{
    public class MakeAccidentCountMapper : IMapperFunc
    {
        public KeyValuePairCollection Map(string line)
        {
            return new KeyValuePairCollection {
                new CountKvp
                {
                    Key = line.Split(',')[22], // make is 23rd column
                    Value = 1
                }
            };
        }
    }
}