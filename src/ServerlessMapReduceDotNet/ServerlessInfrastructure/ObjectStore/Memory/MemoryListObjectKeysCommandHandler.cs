using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.Model.ObjectStore;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.Memory
{
    internal class MemoryListObjectKeysCommandHandler : ICommandHandler<ListObjectKeysCommand, IReadOnlyCollection<ListedObject>>
    {
        private readonly IMemoryObjectStore _memoryObjectStore;

        public MemoryListObjectKeysCommandHandler(IMemoryObjectStore memoryObjectStore)
        {
            _memoryObjectStore = memoryObjectStore;
        }

        public Task<IReadOnlyCollection<ListedObject>> ExecuteAsync(ListObjectKeysCommand command, IReadOnlyCollection<ListedObject> previousResult)
        {
            var listedObjects = new List<ListedObject>();

            foreach (var storedObject in _memoryObjectStore)
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