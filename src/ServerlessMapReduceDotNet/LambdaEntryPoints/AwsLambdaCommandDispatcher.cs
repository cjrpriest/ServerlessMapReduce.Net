using System.Threading;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using AzureFromTheTrenches.Commanding.Abstractions.Model;

namespace ServerlessMapReduceDotNet.LambdaEntryPoints
{
    internal class AwsLambdaCommandDispatcher : ICommandDispatcher, IFrameworkCommandDispatcher
    {
        public ICommandExecuter AssociatedExecuter { get; }

        public AwsLambdaCommandDispatcher(ICommandExecuter awsLambdaCommandExecuter)
        {
            this.AssociatedExecuter = awsLambdaCommandExecuter;
        }

        public Task<CommandResult<TResult>> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default (CancellationToken))
        {
            return Task.FromResult(new CommandResult<TResult>(default (TResult), false));
        }

        public Task<CommandResult> DispatchAsync(ICommand command, CancellationToken cancellationToken = default (CancellationToken))
        {
            return Task.FromResult(new CommandResult(false));
        }
    }
}
