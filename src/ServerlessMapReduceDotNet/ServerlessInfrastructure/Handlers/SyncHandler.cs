using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Handlers
{
    public class SyncHandler<TFunction, TCommand> : Handler<TFunction, TCommand>
        where TFunction : IFireAndForgetFunction
        where TCommand : ICommand
    {
        public SyncHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            FunctionHandler = async function => { await function.InvokeAsync(); };
        }
    }
}