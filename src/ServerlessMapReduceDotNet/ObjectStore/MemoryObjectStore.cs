using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ServerlessMapReduceDotNet.Abstractions;

namespace ServerlessMapReduceDotNet.ObjectStore
{
    public class MemoryObjectStore : IObjectStore
    {
        private readonly ITime _time;
        private readonly ConcurrentDictionary<string, StoredObject> _store = new ConcurrentDictionary<string, StoredObject>();

        public MemoryObjectStore(ITime time)
        {
            _time = time;
        }

        public Task StoreAsync(string key, Stream dataStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                dataStream.Position = 0;
                dataStream.CopyTo(memoryStream);
                var newStoredObject = new StoredObject
                {
                    LastModified = _time.UtcNow,
                    Data = memoryStream.ToArray()
                };
                _store.AddOrUpdate(
                    key,
                    newStoredObject,
                    (k, v) => newStoredObject
                );
                Console.WriteLine($"Wrote {dataStream.Length} bytes to {key}");
            }

            return Task.CompletedTask;
        }
        
        public async Task<Stream> RetrieveAsync(string key)
        {
            if (!_store.ContainsKey(key))
                throw new InvalidOperationException($"Object stored in key [{key}] could not be found");
            
            var memoryStream = new MemoryStream(_store[key].Data);
            return memoryStream;
        }

        public Task<IReadOnlyCollection<ListedObject>> ListKeysPrefixedAsync(string prefix)
        {
            var listedObjects = new List<ListedObject>();

            foreach (var storedObject in _store)
            {
                if (storedObject.Key.StartsWith(prefix))
                {
                    listedObjects.Add(new ListedObject
                    {
                        Key = storedObject.Key,
                        LastModified = storedObject.Value.LastModified
                    });
                }
            }

            return Task.FromResult<IReadOnlyCollection<ListedObject>>(listedObjects);
        }
    }
}