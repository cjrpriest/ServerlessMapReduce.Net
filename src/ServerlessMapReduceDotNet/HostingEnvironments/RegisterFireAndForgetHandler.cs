using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.HostingEnvironments
{
    class RegisterFireAndForgetHandler : IRegisterFireAndForgetHandler
    {
        private readonly ICommandRegistry _commandRegistry;
        private readonly IServiceCollection _serviceCollection;
        private readonly Type _functionHandlerType;
        private readonly ICommandDispatcher _customCommandDispatcher;

        public RegisterFireAndForgetHandler(ICommandRegistry commandRegistry, IServiceCollection serviceCollection, Type functionHandlerType, ICommandDispatcher customCommandDispatcher)
        {
            _commandRegistry = commandRegistry;
            _serviceCollection = serviceCollection;
            _functionHandlerType = functionHandlerType;
            _customCommandDispatcher = customCommandDispatcher;
        }

        public IRegisterFireAndForgetHandler RegisterFireAndForgetFunctionImpl<TFunction, TCommand>()
            where TFunction : class, IFireAndForgetFunction
            where TCommand : ICommand
        {
            _serviceCollection.AddTransient<TFunction>();
            
            var commandHandlerType = _functionHandlerType.MakeGenericType(typeof(TFunction), typeof(TCommand));

            _commandRegistry.Register(commandHandlerType);
            if (_customCommandDispatcher != null)
                _commandRegistry.Register<TCommand>(() => _customCommandDispatcher);
            return this;
        }
    }
}