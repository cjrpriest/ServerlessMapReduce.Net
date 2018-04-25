using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.ObjectStore;

namespace ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory
{
    internal class MemoryListObjectKeysCommandHandler : ICommandHandler<ListObjectKeysCommand, IReadOnlyCollection<ListedObject>>
    {
        private readonly IMemoryObjectStoreData _memoryObjectStoreData;

        public MemoryListObjectKeysCommandHandler(IMemoryObjectStoreData memoryObjectStoreData)
        {
            _memoryObjectStoreData = memoryObjectStoreData;
        }

        public Task<IReadOnlyCollection<ListedObject>> ExecuteAsync(ListObjectKeysCommand command, IReadOnlyCollection<ListedObject> previousResult)
        {
            var listedObjects = new List<ListedObject>();

            foreach (var storedObject in _memoryObjectStoreData)
            {
                if (storedObject.Key.StartsWith(command.Prefix))
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