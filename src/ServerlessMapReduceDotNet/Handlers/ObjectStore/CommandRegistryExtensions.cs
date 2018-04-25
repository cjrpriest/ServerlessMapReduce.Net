using AzureFromTheTrenches.Commanding.Abstractions;
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
    }
}