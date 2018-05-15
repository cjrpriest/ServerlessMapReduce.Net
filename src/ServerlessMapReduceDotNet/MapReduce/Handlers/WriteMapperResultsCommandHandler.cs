using System.IO;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Newtonsoft.Json;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.MapReduce.Helpers;

namespace ServerlessMapReduceDotNet.MapReduce.Handlers
{
    public class WriteMapperResultsCommandHandler : ICommandHandler<WriteMapperResultsCommand>
    {
        private readonly IConfig _config;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueueClient _queueClient;

        public WriteMapperResultsCommandHandler(IConfig config, ICommandDispatcher commandDispatcher, IQueueClient queueClient)
        {
            _config = config;
            _commandDispatcher = commandDispatcher;
            _queueClient = queueClient;
        }
        
        public async Task ExecuteAsync(WriteMapperResultsCommand command)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                if (command.ResultOfMap.Count > 0)
                {
                    var keyValuePairsJson = JsonConvert.SerializeObject(command.ResultOfMap, Formatting.None,
                        new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});
                    await streamWriter.WriteLineAsync(keyValuePairsJson);
                }

                await streamWriter.FlushAsync();
                var mapperOutputKey = $"{_config.MappedFolder}/{command.ContextQueueMessage.Message.ObjectName()}";
                await _commandDispatcher.DispatchAsync(new StoreObjectCommand
                {
                    Key = mapperOutputKey,
                    DataStream = memoryStream
                });
                await _queueClient.Enqueue(_config.MappedQueueName, mapperOutputKey);

                await _queueClient.MessageProcessed(_config.IngestedQueueName, command.ContextQueueMessage.MessageId);
            }
        }
    }
}