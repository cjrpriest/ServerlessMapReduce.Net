using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.Memory
{
    class MemoryObjectStore : IMemoryObjectStore
    {
        private readonly ConcurrentDictionary<string, StoredObject> _store = new ConcurrentDictionary<string, StoredObject>();

        
        public StoredObject Retrieve(string key)
        {
            if (!_store.ContainsKey(key))
                throw new InvalidOperationException($"Object stored in key [{key}] could not be found");
            
            return _store[key];
        }

        public void Add(string key, StoredObject storeObject)
        {
            _store.AddOrUpdate(
                key,
                storeObject,
                (k, v) => storeObject
            );
        }
        
        public IEnumerator<KeyValuePair<string, StoredObject>> GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}