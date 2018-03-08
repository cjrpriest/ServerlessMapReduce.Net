using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ServerlessMapReduceDotNet.Abstractions;

namespace ServerlessMapReduceDotNet.Functions
{
    public class Ingester : IIngester
    {
        private readonly IObjectStore _objectStore;
        private readonly IQueueClient _queueClient;
        private readonly IConfig _config;
        private readonly IWorkerRecordStoreService _workerRecordStoreService;

        private readonly Regex _keyRegex = new Regex(@".*/(?<objectName>.*?)$", RegexOptions.Compiled);
        
        public Ingester(IObjectStore objectStore, IQueueClient queueClient, IConfig config, IWorkerRecordStoreService workerRecordStoreService)
        {
            _objectStore = objectStore;
            _queueClient = queueClient;
            _config = config;
            _workerRecordStoreService = workerRecordStoreService;
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
                var rawDataObjectStream = await _objectStore.RetrieveAsync(rawDataQueueMessage.Message);
                var objectName = _keyRegex.Match(rawDataObjectKey).Groups["objectName"].Value;
                
                using (var objectReader = new StreamReader(rawDataObjectStream))
                {
                    while (!objectReader.EndOfStream)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            var ingestedObjectKey =
                                $"{_config.IngestedFolder}/{objectName}-{Guid.NewGuid()}"; 

                            using (var sw = new StreamWriter(memoryStream))
                            {
                                for (int i = 0; i < _config.IngesterMaxLinesPerFile && !objectReader.EndOfStream; i++)
                                {
                                    var line = await objectReader.ReadLineAsync();
                                    await sw.WriteLineAsync(line);
                                }

                                await sw.FlushAsync();
                                await _objectStore.StoreAsync(ingestedObjectKey, memoryStream);
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