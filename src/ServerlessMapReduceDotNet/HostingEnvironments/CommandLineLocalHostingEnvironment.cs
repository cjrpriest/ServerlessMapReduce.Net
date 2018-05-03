using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Configuration;
using ServerlessMapReduceDotNet.Handlers;
using ServerlessMapReduceDotNet.Handlers.ObjectStore;
using ServerlessMapReduceDotNet.Handlers.Terminate;
using ServerlessMapReduceDotNet.Queue.InMemory;

namespace ServerlessMapReduceDotNet.HostingEnvironments
{
    public class CommandLineLocalHostingEnvironment : HostingEnvironment
    {
        public override IQueueClient QueueClientFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<InMemoryQueueClient>();
        
        public override IConfig ConfigFactory() => new Config();
        
        public override Type TerminatorHandlerTypeFactory() => typeof(TerminateCommandHandler);

        protected override HostingEnvironment RegisterFireAndForgetFunctionImpl<TFunction, TCommand>()
        {
            CommandRegistry.Register<AsyncHandler<TFunction, TCommand>>();
            return this;
        }

        protected override void RegisterObjectStoreImpl(ICommandRegistry cr) => cr.RegisterFileSystemObjectStore();
    }
}