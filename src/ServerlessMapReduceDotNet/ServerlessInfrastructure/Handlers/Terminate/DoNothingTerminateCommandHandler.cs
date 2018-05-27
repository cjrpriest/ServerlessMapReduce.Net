using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Handlers.Terminate
{
    class DoNothingTerminateCommandHandler : ICommandHandler<TerminateCommand>
    {
        public Task ExecuteAsync(TerminateCommand command)
        {
            // do nothing
            return Task.CompletedTask;
        }
    }
}