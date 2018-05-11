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
        protected override IQueueClient QueueClientFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<InMemoryQueueClient>();
        
        public override IConfig ConfigFactory() => new Config();

        protected override Type TerminatorHandlerTypeFactory() => typeof(TerminateCommandHandler);

        protected override Type FireAndForgetHandlerType() => typeof(AsyncHandler<,>);

        protected override void RegisterObjectStoreImpl(ICommandRegistry cr) => cr.RegisterFileSystemObjectStore();
    }
}