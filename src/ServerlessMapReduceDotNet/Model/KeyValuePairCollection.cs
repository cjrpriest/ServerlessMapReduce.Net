using System.Collections.Generic;

namespace ServerlessMapReduceDotNet.Model
{
    public class KeyValuePairCollection : List<IKeyValuePair>
    {
        public KeyValuePairCollection() { }
        
        public KeyValuePairCollection(IEnumerable<IKeyValuePair> keyValuePairs)
        {
            AddRange(keyValuePairs);
        }
    }
}