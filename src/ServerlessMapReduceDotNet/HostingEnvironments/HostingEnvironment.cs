using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Abstractions;

namespace ServerlessMapReduceDotNet.HostingEnvironments
{
    public abstract class HostingEnvironment
    {
        public ICommandRegistry CommandRegistry { protected get; set; }
        public IServiceCollection ServiceCollection { private get; set; }
        
        public abstract IQueueClient QueueClientFactory(IServiceProvider serviceProvider);

        public abstract IConfig ConfigFactory();

        public abstract Type TerminatorHandlerTypeFactory();

        public HostingEnvironment RegisterFireAndForgetFunction<TFunction, TCommand>()
            where TFunction : class, IFireAndForgetFunction 
            where TCommand : ICommand
        {
            ServiceCollection.AddTransient<TFunction>();
            return RegisterFireAndForgetFunctionImpl<TFunction, TCommand>();
        }
        
        protected abstract HostingEnvironment RegisterFireAndForgetFunctionImpl<TFunction, TCommand>()
            where TFunction : IFireAndForgetFunction
            where TCommand : ICommand;

        public HostingEnvironment RegisterMiscHandlers()
        {
            return RegisterMiscHandlersImpl(CommandRegistry);
        }

        protected virtual HostingEnvironment RegisterMiscHandlersImpl(ICommandRegistry commandRegistry)
        {
            return this;
        }

        public HostingEnvironment RegisterObjectStore()
        {
            RegisterObjectStoreImpl(CommandRegistry);
            return this;
        }

        protected abstract void RegisterObjectStoreImpl(ICommandRegistry cr);
    }

    public static class CommandRegistryExtensions
    {
        public static ICommandRegistry RegisterHostingEnvironment(this ICommandRegistry commandRegistry, HostingEnvironment hostingEnvironment)
        {
            hostingEnvironment.CommandRegistry = commandRegistry;
            commandRegistry.Register(hostingEnvironment.TerminatorHandlerTypeFactory());
            return commandRegistry;
        }
        
        public static IServiceCollection RegisterHostingEnvironment(this IServiceCollection serviceCollection, HostingEnvironment hostingEnvironment)
        {
            hostingEnvironment.ServiceCollection = serviceCollection;

            serviceCollection.AddSingleton(hostingEnvironment.QueueClientFactory);
            serviceCollection.AddSingleton(x => hostingEnvironment.ConfigFactory());
            
            return serviceCollection;
        }
    }
}