using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions
{
    interface IMemoryObjectStore : IConcurrentKeyValueStore<string, StoredObject> { }
}