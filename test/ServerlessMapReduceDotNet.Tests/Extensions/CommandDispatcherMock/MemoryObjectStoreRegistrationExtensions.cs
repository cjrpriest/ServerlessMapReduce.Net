using System.Collections.Generic;
using System.IO;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory;
using ServerlessMapReduceDotNet.ObjectStore;
using ServerlessMapReduceDotNet.Services;

namespace ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock
{
    internal static class MemoryObjectStoreRegistrationExtensions
    {
        public static ICommandDispatcher RegisterMemoryObjectStore(this ICommandDispatcher commandDispatcherMock, ITime timeMock)
        {
            var memoryObjectStoreData = new MemoryObjectStoreData();

            commandDispatcherMock
                .Register<ListObjectKeysCommand, IReadOnlyCollection<ListedObject>, MemoryListObjectKeysCommandHandler>(
                    () => new MemoryListObjectKeysCommandHandler(memoryObjectStoreData))
                .Register<RetrieveObjectCommand, Stream, MemoryRetrieveObjectCommandHandler>(() =>
                    new MemoryRetrieveObjectCommandHandler(memoryObjectStoreData))
                .Register<StoreObjectCommand, MemoryStoreObjectCommandHandler>(() => new MemoryStoreObjectCommandHandler(timeMock, memoryObjectStoreData));

            return commandDispatcherMock;
        }
    }
}