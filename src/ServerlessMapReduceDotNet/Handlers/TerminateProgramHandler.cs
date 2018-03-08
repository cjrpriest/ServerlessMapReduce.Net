using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;

namespace ServerlessMapReduceDotNet.Handlers
{
    public class TerminateProgramHandler : ICommandHandler<TerminateProgramCommand>
    {
        private readonly ITerminator _terminator;

        public TerminateProgramHandler(ITerminator terminator)
        {
            _terminator = terminator;
        }
        
        public Task ExecuteAsync(TerminateProgramCommand command)
        {
            _terminator.Terminate();
            return Task.CompletedTask;
        }
    }
}