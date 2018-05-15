using System;
using System.IO;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory
{
    class MemoryStoreObjectCommandHandler : ICommandHandler<StoreObjectCommand>
    {
        private readonly ITime _time;
        private readonly IMemoryObjectStoreData _memoryObjectStoreData;

        public MemoryStoreObjectCommandHandler(ITime time, IMemoryObjectStoreData memoryObjectStoreData)
        {
            _time = time;
            _memoryObjectStoreData = memoryObjectStoreData;
        }

        public Task ExecuteAsync(StoreObjectCommand command)
        {
            using (var memoryStream = new MemoryStream())
            {
                command.DataStream.Position = 0;
                command.DataStream.CopyTo(memoryStream);
                var newStoredObject = new StoredObject
                {
                    LastModified = _time.UtcNow,
                    Data = memoryStream.ToArray()
                };
                _memoryObjectStoreData.Add(
                    command.Key,
                    newStoredObject
                );
                Console.WriteLine($"Wrote {command.DataStream.Length} bytes to {command.Key}");
            }

            return Task.CompletedTask;        
        }
    }
}