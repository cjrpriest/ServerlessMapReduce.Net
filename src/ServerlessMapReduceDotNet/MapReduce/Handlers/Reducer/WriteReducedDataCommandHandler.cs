using System.IO;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Newtonsoft.Json;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.MapReduce.Commands.Reduce;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.MapReduce.Handlers.Reducer
{
    public class WriteReducedDataCommandHandler : ICommandHandler<WriteReducedDataCommand>
    {
        private readonly IConfig _config;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueueClient _queueClient;

        public WriteReducedDataCommandHandler(IConfig config, ICommandDispatcher commandDispatcher, IQueueClient queueClient)
        {
            _config = config;
            _commandDispatcher = commandDispatcher;
            _queueClient = queueClient;
        }
        
        public async Task ExecuteAsync(WriteReducedDataCommand command)
        {
            var reducedCountsJson = JsonConvert.SerializeObject(command.ReducedData, Formatting.None,
                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});
            
            using (var reducedDataMemoryStream = new MemoryStream())
            using (var reducedDataStreamWriter = new StreamWriter(reducedDataMemoryStream))
            {
                
                await reducedDataStreamWriter.WriteLineAsync(reducedCountsJson);

                await reducedDataStreamWriter.FlushAsync();
                var reducedOutputKey = $"{_config.ReducedFolder}/{command.ProcessedMessageIdsHash}";
                
                // Without this being in a transaction, there is the risk of incorrect results
                await _commandDispatcher.DispatchAsync(new StoreObjectCommand
                {
                    Key = reducedOutputKey,
                    DataStream = reducedDataMemoryStream
                });
                await _queueClient.Enqueue(_config.ReducedQueueName, reducedOutputKey);
                // Transaction end
            }
        }
    }
}