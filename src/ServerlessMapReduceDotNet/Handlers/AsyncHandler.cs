using System;
using System.Threading;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.Handlers
{
    public class AsyncHandler<TFunction, TCommand> : Handler<TFunction, TCommand>
        where TFunction : IFireAndForgetFunction
        where TCommand : ICommand
    {
        public AsyncHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            FunctionHandler = function =>
            {
                new Thread(() => { ExceptionHelper.LogExceptionAndContinue(() => function.InvokeAsync().Wait()); })
                    .Start();
                return Task.CompletedTask;
            };
        }
    }
}