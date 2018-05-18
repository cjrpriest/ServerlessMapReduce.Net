using System;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Execution
{
    class QueueCommandDispatcher : CommandDispatcherBase
    {
        private readonly IServiceProvider _serviceProvider;

        public QueueCommandDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override ICommandExecuter AssociatedExecuter => _serviceProvider.GetService<QueueCommandExecuter>();
    }
}