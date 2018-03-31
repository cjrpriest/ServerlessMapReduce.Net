using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands;

namespace ServerlessMapReduceDotNet.Handlers.Terminate
{
    class TerminateCommandHandler : TerminateCommandHandlerBase, ICommandHandler<TerminateCommand>
    {
        public Task ExecuteAsync(TerminateCommand command)
        {
            IsTerminated = true;
            return Task.CompletedTask;
        }
    }
}