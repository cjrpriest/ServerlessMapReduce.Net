using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.AmazonS3;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.FileSystem;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.Memory;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore
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