using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Handlers.Terminate
{
    class CommandLineTerminateCommandHandler : TerminateCommandHandlerBase, ICommandHandler<TerminateCommand>
    {
        public Task ExecuteAsync(TerminateCommand command)
        {
            IsTerminated = true;
            return Task.CompletedTask;
        }
    }
}