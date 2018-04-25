﻿using System.IO;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;

namespace ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory
{
    class MemoryRetrieveObjectCommandHandler : ICommandHandler<RetrieveObjectCommand, Stream>
    {
        private readonly IMemoryObjectStoreData _memoryObjectStoreData;

        public MemoryRetrieveObjectCommandHandler(IMemoryObjectStoreData memoryObjectStoreData)
        {
            _memoryObjectStoreData = memoryObjectStoreData;
        }

        public Task<Stream> ExecuteAsync(RetrieveObjectCommand command, Stream previousResult)
        {
            var storedObject = _memoryObjectStoreData.Retrieve(command.Key);
            var memoryStream = new MemoryStream(storedObject.Data);
            return Task.FromResult((Stream)memoryStream);
        }
    }
}