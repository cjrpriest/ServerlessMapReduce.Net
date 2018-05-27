using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Configuration;
using ServerlessMapReduceDotNet.MapReduce.Commands.Map;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Execution;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Handlers;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Handlers.Terminate;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Queue.AmazonSqs;

namespace ServerlessMapReduceDotNet.HostingEnvironments
{
    public class AwsLambdaHostingEnvironment : HostingEnvironment
    {
        protected override IQueueClient QueueClientFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<AmazonSqsQueueClient>();

        public override IConfig ConfigFactory() => new Config();

        protected override Type TerminatorHandlerTypeFactory() => typeof(DoNothingTerminateCommandHandler);

        protected override ICommandDispatcher CustomCommandDispatcherFactory() => new AwsLambdaCommandDispatcher(new AwsLambdaCommandExecuter());

        protected override Type FireAndForgetHandlerType() => typeof(SyncHandler<,>);

        protected override void RegisterObjectStoreImpl(ICommandRegistry cr) => cr.RegisterAmazonS3ObjectStore();
        
        protected override void RegisterMiscHandlersImpl(ICommandRegistry commandRegistry, Func<IServiceProvider> serviceProviderFactory)
        {
            commandRegistry.Register<BatchMapDataCommand>(() => serviceProviderFactory().GetService<QueueCommandDispatcher>());
            commandRegistry.Register<WriteMappedDataCommand>(() => serviceProviderFactory().GetService<QueueCommandDispatcher>());
        }
    }
}
