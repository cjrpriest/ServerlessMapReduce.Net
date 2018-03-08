using System.Collections.Generic;
using System.Linq;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Reducers
{
    class MakeAccidentCountReducer : IReducerFunc
    {
        public KeyValuePairCollection Reduce(KeyValuePairCollection inputKeyValuePairs)
        {
            var reducedCounts = new Dictionary<string, int>();
            inputKeyValuePairs.ForEach(x =>
            {
                var count = (CountKvp)x;
                if (!reducedCounts.ContainsKey(count.Key))
                    reducedCounts.Add(count.Key, 0);
                reducedCounts[count.Key] = reducedCounts[count.Key] + count.Value;                
            });
            
            var keyValuePairs = new KeyValuePairCollection();

            reducedCounts.ToList().ForEach(x => keyValuePairs.Add(new CountKvp(x.Key, x.Value)));

            return keyValuePairs;
        }
    }
}