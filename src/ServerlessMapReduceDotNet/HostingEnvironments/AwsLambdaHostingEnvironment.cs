using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Configuration;
using ServerlessMapReduceDotNet.Handlers;
using ServerlessMapReduceDotNet.LambdaEntryPoints;
using ServerlessMapReduceDotNet.ObjectStore.AmazonS3;
using ServerlessMapReduceDotNet.Queue.AmazonSqs;

namespace ServerlessMapReduceDotNet.HostingEnvironments
{
    public class AwsLambdaHostingEnvironment : HostingEnvironment
    {
        public override IObjectStore ObjectStoreFactory(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<AmazonS3ObjectStore>();
        }

        public override IQueueClient QueueClientFactory(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<AmazonSqsQueueClient>();
        }

        public override IConfig ConfigFactory()
        {
            return new Config();
        }

        public override ITerminator TerminatorFactory()
        {
            return new AwsLambdaTerminator();
        }

        protected override HostingEnvironment RegisterFireAndForgetFunctionImpl<TFunction, TCommand>()
        {
            CommandRegistry.Register<SyncHandler<TFunction, TCommand>>();
            CommandRegistry.Register<TCommand>(() => (ICommandDispatcher) new AwsLambdaCommandDispatcher(new AwsLambdaCommandExecuter()));
            return this;
        }
    }
}
