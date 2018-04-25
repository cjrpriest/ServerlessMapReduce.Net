using System.Collections.Generic;

namespace ServerlessMapReduceDotNet.Abstractions
{
    interface IConcurrentKeyValueStore<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        TValue Retrieve(TKey key);
        void Add(TKey key, TValue storeObject);
    }
}