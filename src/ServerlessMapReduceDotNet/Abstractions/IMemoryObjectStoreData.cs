using ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory;

namespace ServerlessMapReduceDotNet.Abstractions
{
    interface IMemoryObjectStoreData : IConcurrentKeyValueStore<string, StoredObject> { }
}