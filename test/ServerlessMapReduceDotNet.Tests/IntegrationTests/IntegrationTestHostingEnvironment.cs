using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.HostingEnvironments;
using ServerlessMapReduceDotNet.MapReduce.Commands.Map;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Execution;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Handlers;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Handlers.Terminate;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Queue.InMemory;

namespace ServerlessMapReduceDotNet.Tests.IntegrationTests
{
    class IntegrationTestHostingEnvironment : HostingEnvironment
    {
        protected override IQueueClient QueueClientFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<InMemoryQueueClient>();

        public override IConfig ConfigFactory() => new IntegrationTestConfig();

        protected override Type TerminatorHandlerTypeFactory() => typeof(CommandLineTerminateCommandHandler);

        protected override Type FireAndForgetHandlerType() => typeof(SyncHandler<,>);

        protected override void RegisterObjectStoreImpl(ICommandRegistry cr) => cr.RegisterMemoryObjectStore();

        protected override void RegisterMiscHandlersImpl(ICommandRegistry commandRegistry,
            Func<IServiceProvider> serviceProviderFactory)
        {
            commandRegistry.Register<BatchMapDataCommand>(() => serviceProviderFactory().GetService<QueueCommandDispatcher>());
            commandRegistry.Register<WriteMappedDataCommand>(() => serviceProviderFactory().GetService<QueueCommandDispatcher>());
        }
    }
}