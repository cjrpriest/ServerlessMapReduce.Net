using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Handlers;
using ServerlessMapReduceDotNet.Handlers.ObjectStore;
using ServerlessMapReduceDotNet.Handlers.Terminate;
using ServerlessMapReduceDotNet.HostingEnvironments;
using ServerlessMapReduceDotNet.Queue.InMemory;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.Tests.IntegrationTests
{
    class IntegrationTestHostingEnvironment : HostingEnvironment
    {
        protected override IQueueClient QueueClientFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<InMemoryQueueClient>();

        public override IConfig ConfigFactory() => new IntegrationTestConfig();

        protected override Type TerminatorHandlerTypeFactory() => typeof(TerminateCommandHandler);

        protected override Type FireAndForgetHandlerType() => typeof(SyncHandler<,>);

        protected override void RegisterObjectStoreImpl(ICommandRegistry cr) => cr.RegisterMemoryObjectStore();
    }
}