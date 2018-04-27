using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Handlers.ObjectStore.AmazonS3;
using ServerlessMapReduceDotNet.Handlers.ObjectStore.FileSystem;
using ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory;

namespace ServerlessMapReduceDotNet.Handlers.ObjectStore
{
    public static class CommandRegistryExtensions
    {
        public static ICommandRegistry RegisterMemoryObjectStore(this ICommandRegistry commandRegistry)
        {
            return commandRegistry
                .Register<MemoryStoreObjectCommandHandler>()
                .Register<MemoryRetrieveObjectCommandHandler>()
                .Register<MemoryListObjectKeysCommandHandler>();
        }
        
        public static ICommandRegistry RegisterFileSystemObjectStore(this ICommandRegistry commandRegistry)
        {
            return commandRegistry
                .Register<FileSystemStoreObjectCommandHandler>()
                .Register<FileSystemRetrieveObjectCommandHandler>()
                .Register<FileSystemListObjectKeysCommandHandler>();
        }
        
        public static ICommandRegistry RegisterAmazonS3ObjectStore(this ICommandRegistry commandRegistry)
        {
            return commandRegistry
                .Register<AmazonS3StoreObjectCommandHandler>()
                .Register<AmazonS3RetrieveObjectCommandHandler>()
                .Register<AmazonS3ListObjectKeysCommandHandler>();
        }
    }
}