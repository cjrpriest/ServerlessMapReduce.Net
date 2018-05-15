using ServerlessMapReduceDotNet.Handlers.ObjectStore;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions
{
    interface IMemoryObjectStoreData : IConcurrentKeyValueStore<string, StoredObject> { }
}