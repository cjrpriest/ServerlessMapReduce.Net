using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.Handlers.ObjectStore.FileSystem;
using ServerlessMapReduceDotNet.ObjectStore.FileSystem;

namespace ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock
{
    public static class FileSystemObjectStoreRegistrationExtensions
    {
        public static ICommandDispatcher RegisterFileSystemObjectStore(this ICommandDispatcher commandDispatcherMock, ITime timeMock, IFileObjectStoreConfig config, IFileSystem fileSystem)
        {
            commandDispatcherMock
                .Register(new FileSystemListObjectKeysCommandHandler(config, fileSystem))
                .Register(new FileSystemRetrieveObjectCommandHandler(config))
                .Register<StoreObjectCommand, FileSystemStoreObjectCommandHandler>(() => new FileSystemStoreObjectCommandHandler(config, timeMock));

            return commandDispatcherMock;
        }
    }
}
