using System;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Configuration;
using ServerlessMapReduceDotNet.Handlers;
using ServerlessMapReduceDotNet.Handlers.Terminate;
using ServerlessMapReduceDotNet.ObjectStore.AmazonS3;
using ServerlessMapReduceDotNet.Queue.AmazonSqs;

namespace ServerlessMapReduceDotNet.HostingEnvironments
{
    public class CommandLineAwsHostingEnvironment : HostingEnvironment
    {
        public override IObjectStore ObjectStoreFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<AmazonS3ObjectStore>();

        public override IQueueClient QueueClientFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<AmazonSqsQueueClient>();
        
        public override IConfig ConfigFactory() => new Config();
        
        public override Type TerminatorHandlerTypeFactory() => typeof(TerminateCommandHandler);

        protected override HostingEnvironment RegisterFireAndForgetFunctionImpl<TFunction, TCommand>()
        {
            CommandRegistry.Register<AsyncHandler<TFunction, TCommand>>();
            return this;
        }
    }
}