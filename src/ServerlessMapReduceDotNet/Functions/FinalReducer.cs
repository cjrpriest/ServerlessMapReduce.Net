using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.FinalReducers;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Functions
{
    public class FinalReducer : IFinalReducer
    {
        private readonly IObjectStore _objectStore;
        private readonly IQueueClient _queueClient;
        private readonly IConfig _config;
        private readonly IWorkerRecordStoreService _workerRecordStoreService;

        public FinalReducer(IObjectStore objectStore, IQueueClient queueClient, IConfig config, IWorkerRecordStoreService workerRecordStoreService)
        {
            _objectStore = objectStore;
            _queueClient = queueClient;
            _config = config;
            _workerRecordStoreService = workerRecordStoreService;
        }

        public async Task InvokeAsync()
        {
            var instanceWorkerId = _workerRecordStoreService.GenerateUniqueId();
            await _workerRecordStoreService.RecordPing("finalReducer", instanceWorkerId);
            
            var reducedQueueMessages = await _queueClient.Dequeue(_config.ReducedQueueName, _config.MaxQueueItemsBatchSizeToProcessPerWorker);
            if (reducedQueueMessages.Count != 1)
            {
                await _workerRecordStoreService.RecordHasTerminated("finalReducer", instanceWorkerId);
                //TODO consider logging and reutrning without throwing an exception
                throw new ApplicationException("Expected to find just one message on the reducer queue");
            }
            
            var reducedQueueMessage = reducedQueueMessages.First();

            using (var streamReader = new StreamReader(await _objectStore.RetrieveAsync($"{reducedQueueMessage.Message}")))
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                if (!streamReader.EndOfStream)
                {
                    var line = await streamReader.ReadLineAsync();
                    var keyValuePairs = JsonConvert.DeserializeObject<KeyValuePairCollection>(line,
                        new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

                    foreach (var keyValuePair in keyValuePairs)
                    {
                        var linesToWrite = new MakeAccidentCountFinalReduce().FinalReduce(keyValuePair);
                        foreach (var lineToWrite in linesToWrite)
                            await streamWriter.WriteLineAsync(lineToWrite);
                    }

                    await streamWriter.FlushAsync();

                    var finalObjectKey = $"{_config.FinalReducedFolder}/{Guid.NewGuid()}";
                    await _objectStore.StoreAsync(finalObjectKey, memoryStream);
                    await _queueClient.Enqueue(_config.FinalReducedQueueName, finalObjectKey);
                }
            }

            await _queueClient.MessageProcessed(_config.ReducedQueueName, reducedQueueMessage.MessageId);
            await _workerRecordStoreService.RecordHasTerminated("finalReducer", instanceWorkerId);
        }
    }
}