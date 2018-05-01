using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Handlers.ObjectStore.FileSystem;
using ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory;
using ServerlessMapReduceDotNet.ObjectStore.FileSystem;
using ServerlessMapReduceDotNet.Services;

namespace ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock
{
    internal static class ObjectStoreRegistrationExtensions
    {
        public static ICommandDispatcher RegisterMemoryObjectStore(this ICommandDispatcher commandDispatcherMock, ITime timeMock)
        {
            var memoryObjectStoreData = new MemoryObjectStoreData();

            commandDispatcherMock
                .Register(new MemoryListObjectKeysCommandHandler(memoryObjectStoreData))
                .Register(new MemoryRetrieveObjectCommandHandler(memoryObjectStoreData))
                .Register(new MemoryStoreObjectCommandHandler(timeMock, memoryObjectStoreData));

            return commandDispatcherMock;
        }
        
        public static ICommandDispatcher RegisterFileSystemObjectStore(this ICommandDispatcher commandDispatcherMock, ITime timeMock, IFileObjectStoreConfig config, IFileSystem fileSystem)
        {
            commandDispatcherMock
                .Register(new FileSystemListObjectKeysCommandHandler(config, fileSystem))
                .Register(new FileSystemRetrieveObjectCommandHandler(config))
                .Register(new FileSystemStoreObjectCommandHandler(config, timeMock));

            return commandDispatcherMock;
        }
    }
}