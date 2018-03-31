using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands;

namespace ServerlessMapReduceDotNet.Handlers.Terminate
{
    class IsTerminatedCommandHandler : TerminateCommandHandlerBase, ICommandHandler<IsTerminatedCommand, bool>
    {
        public Task<bool> ExecuteAsync(IsTerminatedCommand command, bool previousResult)
        {
            return Task.FromResult(IsTerminated);
        }
    }
}