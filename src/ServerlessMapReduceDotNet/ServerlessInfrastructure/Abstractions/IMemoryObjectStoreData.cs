using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions
{
    interface IMemoryObjectStoreData : IConcurrentKeyValueStore<string, StoredObject> { }
}