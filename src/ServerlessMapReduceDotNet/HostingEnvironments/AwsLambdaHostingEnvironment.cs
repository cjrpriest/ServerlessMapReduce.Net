using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Configuration;
using ServerlessMapReduceDotNet.EntryPoints.Lambda;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Queue.AmazonSqs;

namespace ServerlessMapReduceDotNet.HostingEnvironments
{
    public class AwsLambdaHostingEnvironment : HostingEnvironment
    {
        protected override IQueueClient QueueClientFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<AmazonSqsQueueClient>();

        public override IConfig ConfigFactory() => new Config();

        protected override Type TerminatorHandlerTypeFactory() => typeof(AwsLambdaTerminator);

        protected override ICommandDispatcher CustomCommandDispatcherFactory()
        {
            return new AwsLambdaCommandDispatcher(new AwsLambdaCommandExecuter());
        }

        protected override Type FireAndForgetHandlerType()
        {
            throw new NotImplementedException();
        }

        protected override void RegisterObjectStoreImpl(ICommandRegistry cr) => cr.RegisterAmazonS3ObjectStore();
    }
}
