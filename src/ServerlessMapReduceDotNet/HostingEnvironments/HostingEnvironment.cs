using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.HostingEnvironments
{
    public abstract class HostingEnvironment
    {
        public abstract IConfig ConfigFactory();

        protected abstract IQueueClient QueueClientFactory(IServiceProvider serviceProvider);

        protected abstract Type TerminatorHandlerTypeFactory();

        protected abstract Type FireAndForgetHandlerType();

        protected virtual ICommandDispatcher CustomCommandDispatcherFactory() => null;

        protected virtual void RegisterMiscHandlersImpl(ICommandRegistry commandRegistry) { }

        protected abstract void RegisterObjectStoreImpl(ICommandRegistry cr);

        public void RegisterHostingEnvironment(ICommandRegistry commandRegistry, IServiceCollection serviceCollection, Action<IRegisterFireAndForgetHandler> registerFireAndForgetHandlers)
        {
            commandRegistry.Register(TerminatorHandlerTypeFactory());
            RegisterMiscHandlersImpl(commandRegistry);
            RegisterObjectStoreImpl(commandRegistry);
            registerFireAndForgetHandlers(new RegisterFireAndForgetHandler(commandRegistry, serviceCollection, FireAndForgetHandlerType(), CustomCommandDispatcherFactory()));
            
            serviceCollection.AddSingleton(QueueClientFactory);
            serviceCollection.AddSingleton(x => ConfigFactory());
        }
    }
}