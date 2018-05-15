using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.MapReduce.FireAndForgetFunctions
{
    public class Ingester : IIngester
    {
        private readonly IQueueClient _queueClient;
        private readonly IConfig _config;
        private readonly IWorkerRecordStoreService _workerRecordStoreService;
        private readonly ICommandDispatcher _commandDispatcher;

        private readonly Regex _keyRegex = new Regex(@".*/(?<objectName>.*?)$", RegexOptions.Compiled);
        
        public Ingester(IQueueClient queueClient, IConfig config, IWorkerRecordStoreService workerRecordStoreService, ICommandDispatcher commandDispatcher)
        {
            _queueClient = queueClient;
            _config = config;
            _workerRecordStoreService = workerRecordStoreService;
            _commandDispatcher = commandDispatcher;
        }

        public async Task InvokeAsync()
        {
            var instanceWorkerId = _workerRecordStoreService.GenerateUniqueId();
            await _workerRecordStoreService.RecordPing("ingester", instanceWorkerId);
            
            var rawDataQueueMessages = await _queueClient.Dequeue(_config.RawDataQueueName, _config.MaxQueueItemsBatchSizeToProcessPerWorker);
            if (rawDataQueueMessages.Count == 0)
            {
                await _workerRecordStoreService.RecordHasTerminated("ingester", instanceWorkerId);
                return;
            }

            foreach (var rawDataQueueMessage in rawDataQueueMessages)
            {
                var rawDataObjectKey = rawDataQueueMessage.Message;
                Stream rawDataObjectStream = await _commandDispatcher.DispatchAsync(new RetrieveObjectCommand{Key = rawDataQueueMessage.Message});
                var objectName = _keyRegex.Match(rawDataObjectKey).Groups["objectName"].Value;
                
                using (var objectReader = new StreamReader(rawDataObjectStream))
                {
                    while (!objectReader.EndOfStream)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            var ingestedObjectKey = $"{_config.IngestedFolder}/{objectName}-{Guid.NewGuid()}"; 

                            using (var sw = new StreamWriter(memoryStream))
                            {
                                for (int i = 0; i < _config.IngesterMaxLinesPerFile && !objectReader.EndOfStream; i++)
                                {
                                    var line = await objectReader.ReadLineAsync();
                                    await sw.WriteLineAsync(line);
                                }

                                await sw.FlushAsync();
                                await _commandDispatcher.DispatchAsync(new StoreObjectCommand
                                {
                                    Key = ingestedObjectKey,
                                    DataStream = memoryStream
                                });
                            }

                            await _queueClient.Enqueue(_config.IngestedQueueName, ingestedObjectKey);
                        }
                    }
                }

                await _queueClient.MessageProcessed(_config.RawDataQueueName, rawDataQueueMessage.MessageId);
            }
            
            await _workerRecordStoreService.RecordHasTerminated("ingester", instanceWorkerId);
        }
    }
}