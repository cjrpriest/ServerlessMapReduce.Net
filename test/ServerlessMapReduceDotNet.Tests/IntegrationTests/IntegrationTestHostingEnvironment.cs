﻿using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Handlers;
using ServerlessMapReduceDotNet.Handlers.ObjectStore;
using ServerlessMapReduceDotNet.Handlers.Terminate;
using ServerlessMapReduceDotNet.HostingEnvironments;
using ServerlessMapReduceDotNet.Queue.InMemory;

namespace ServerlessMapReduceDotNet.Tests.IntegrationTests
{
    public class IntegrationTestHostingEnvironment : HostingEnvironment
    {
        public override IQueueClient QueueClientFactory(IServiceProvider serviceProvider) => serviceProvider.GetService<InMemoryQueueClient>();

        public override IConfig ConfigFactory() => new IntegrationTestConfig();
        
        public override Type TerminatorHandlerTypeFactory() => typeof(TerminateCommandHandler);

        protected override HostingEnvironment RegisterFireAndForgetFunctionImpl<TFunction, TCommand>()
        {
            CommandRegistry.Register<SyncHandler<TFunction, TCommand>>();
            return this;
        }

        protected override HostingEnvironment RegisterMiscHandlersImpl(ICommandRegistry commandRegistry)
        {
            commandRegistry.Register<IsTerminatedCommandHandler>();
            return this;
        }

        protected override void RegisterObjectStoreImpl(ICommandRegistry cr) => cr.RegisterMemoryObjectStore();
    }
}