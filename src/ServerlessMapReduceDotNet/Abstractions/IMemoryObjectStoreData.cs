using ServerlessMapReduceDotNet.Handlers.ObjectStore;

namespace ServerlessMapReduceDotNet.Abstractions
{
    interface IMemoryObjectStoreData : IConcurrentKeyValueStore<string, StoredObject> { }
}