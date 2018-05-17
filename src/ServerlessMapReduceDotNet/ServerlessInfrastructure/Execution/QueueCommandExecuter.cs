using System.Threading;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Newtonsoft.Json;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Execution
{
    public class QueueCommandExecuter : ICommandExecuter
    {
        private readonly IQueueClient _queueClient;

        public QueueCommandExecuter(IQueueClient queueClient)
        {
            _queueClient = queueClient;
        }
        
        public async Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = new CancellationToken())
        {
            var serializerSettings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
            var commandJson = JsonConvert.SerializeObject(command, Formatting.None, serializerSettings);
            await _queueClient.Enqueue("commandQueue", commandJson);
            return default (TResult);
        }
    }
}