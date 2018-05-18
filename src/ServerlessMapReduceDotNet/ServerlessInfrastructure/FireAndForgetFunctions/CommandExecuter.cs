using System;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using AzureFromTheTrenches.Commanding.Abstractions.Model;
using Newtonsoft.Json;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.FireAndForgetFunctions
{
    public class CommandExecuter : IFireAndForgetFunction
    {
        private readonly IQueueClient _queueClient;
        private readonly IConfig _config;
        private readonly IWorkerRecordStoreService _workerRecordStoreService;
        private readonly ICommandDispatcher _dispatch;
        private readonly IDirectCommandExecuter _directCommandExecuter;

        public CommandExecuter(IQueueClient queueClient, IConfig config, IWorkerRecordStoreService workerRecordStoreService, ICommandDispatcher dispatch, IDirectCommandExecuter directCommandExecuter)
        {
            _queueClient = queueClient;
            _config = config;
            _workerRecordStoreService = workerRecordStoreService;
            _dispatch = dispatch;
            _directCommandExecuter = directCommandExecuter;
        }
        
        public async Task InvokeAsync()
        {
            var instanceWorkerId = _workerRecordStoreService.GenerateUniqueId();
            Console.WriteLine($"CommandExecuter {instanceWorkerId} starting...");
            await _workerRecordStoreService.RecordPing("commandExecuter", instanceWorkerId);
            
            var commandQueueMessages = await _queueClient.Dequeue(_config.CommandQueueName, _config.MaxQueueItemsBatchSizeToProcessPerWorker);
            if (commandQueueMessages.Count == 0)
            {
                await _workerRecordStoreService.RecordHasTerminated("commandExecuter", instanceWorkerId);
                return;
            }

            foreach (var commandQueueMessage in commandQueueMessages)
            {
                var serializerSettings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
                var command = JsonConvert.DeserializeObject<NoResultCommandWrapper>(commandQueueMessage.Message, serializerSettings);
                await _directCommandExecuter.ExecuteAsync(command);
                await _queueClient.MessageProcessed(_config.CommandQueueName, commandQueueMessage.MessageId);
            }
            
            await _workerRecordStoreService.RecordHasTerminated("commandExecuter", instanceWorkerId);
            Console.WriteLine($"Mapper {instanceWorkerId} Terminated");
        }
    }
}