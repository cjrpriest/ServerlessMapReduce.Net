using System.IO;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Newtonsoft.Json;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.MapReduce.Commands.Map;
using ServerlessMapReduceDotNet.MapReduce.Helpers;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.MapReduce.Handlers.Mapper
{
    public class WriteMappedDataCommandHandler : ICommandHandler<WriteMappedDataCommand>
    {
        private readonly IConfig _config;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueueClient _queueClient;

        public WriteMappedDataCommandHandler(IConfig config, ICommandDispatcher commandDispatcher, IQueueClient queueClient)
        {
            _config = config;
            _commandDispatcher = commandDispatcher;
            _queueClient = queueClient;
        }
        
        public async Task ExecuteAsync(WriteMappedDataCommand command)
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
            }
        }
    }
}