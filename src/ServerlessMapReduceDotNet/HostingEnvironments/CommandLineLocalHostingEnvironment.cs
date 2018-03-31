using System;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Configuration;
using ServerlessMapReduceDotNet.Handlers;
using ServerlessMapReduceDotNet.Handlers.Terminate;
using ServerlessMapReduceDotNet.ObjectStore.FileSystem;
using ServerlessMapReduceDotNet.Queue.InMemory;

namespace ServerlessMapReduceDotNet.HostingEnvironments
{
    public class CommandLineLocalHostingEnvironment : HostingEnvironment
    {
        public override IObjectStore ObjectStoreFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<FileSystemObjectStore>();

        public override IQueueClient QueueClientFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<InMemoryQueueClient>();
        
        public override IConfig ConfigFactory() => new Config();
        
        public override Type TerminatorHandlerTypeFactory() => typeof(TerminateCommandHandler);

        protected override HostingEnvironment RegisterFireAndForgetFunctionImpl<TFunction, TCommand>()
        {
            CommandRegistry.Register<AsyncHandler<TFunction, TCommand>>();
            return this;
        }
    }
}