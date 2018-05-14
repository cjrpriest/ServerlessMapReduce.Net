using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using NSubstitute;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Handlers.ObjectStore.FileSystem;
using ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory;
using ServerlessMapReduceDotNet.MapReduce.Handlers;
using ServerlessMapReduceDotNet.ObjectStore.FileSystem;
using ServerlessMapReduceDotNet.Services;

namespace ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock
{
    internal static class RegistrationExtensions
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
        
        public static ICommandDispatcher RegisterReducerFunc(this ICommandDispatcher commandDispatcherMock, IReducerFunc reducerFunc)
        {
            var serviceProviderMock = Substitute.For<IServiceProvider>();
            serviceProviderMock.GetService(Arg.Any<Type>())
                .Returns(reducerFunc);
            
            commandDispatcherMock
                .Register(new ReducerFuncHandler(Substitute.For<IConfig>(), serviceProviderMock));

            return commandDispatcherMock;
        }
    }
}