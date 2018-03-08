using System;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Handlers;
using ServerlessMapReduceDotNet.HostingEnvironments;
using ServerlessMapReduceDotNet.ObjectStore;
using ServerlessMapReduceDotNet.Queue.InMemory;

namespace ServerlessMapReduceDotNet.Tests.IntegrationTests
{
    public class IntegrationTestHostingEnvironment : HostingEnvironment
    {
        public override IObjectStore ObjectStoreFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<MemoryObjectStore>();

        public override IQueueClient QueueClientFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<InMemoryQueueClient>();

        public override IConfig ConfigFactory() => new IntegrationTestConfig();
        
        public override ITerminator TerminatorFactory() => new IntegrationTestTerminator();

        protected override HostingEnvironment RegisterFireAndForgetFunctionImpl<TFunction, TCommand>()
        {
            CommandRegistry.Register<SyncHandler<TFunction, TCommand>>();
            return this;
        }
    }
}