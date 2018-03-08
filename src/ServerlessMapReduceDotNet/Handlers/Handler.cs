using System;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Abstractions;

namespace ServerlessMapReduceDotNet.Handlers
{
    public abstract class Handler<TFunction, TCommand> : ICommandHandler<TCommand>
        where TFunction : IFireAndForgetFunction
        where TCommand : ICommand
    {
        private readonly IServiceProvider _serviceProvider;
        protected Func<IFireAndForgetFunction, Task> FunctionHandler;

        protected Handler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ExecuteAsync(TCommand command)
        {
            var function = _serviceProvider.GetService<TFunction>();
            await FunctionHandler(function);
        }
    }
}