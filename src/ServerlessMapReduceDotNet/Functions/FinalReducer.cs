using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Newtonsoft.Json;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.FinalReducers;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Functions
{
    public class FinalReducer : IFinalReducer
    {
        private readonly IQueueClient _queueClient;
        private readonly IConfig _config;
        private readonly IWorkerRecordStoreService _workerRecordStoreService;
        private readonly ICommandDispatcher _commandDispatcher;

        public FinalReducer(IQueueClient queueClient, IConfig config, IWorkerRecordStoreService workerRecordStoreService, ICommandDispatcher commandDispatcher)
        {
            _queueClient = queueClient;
            _config = config;
            _workerRecordStoreService = workerRecordStoreService;
            _commandDispatcher = commandDispatcher;
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

            using (var streamReader = new StreamReader(await _commandDispatcher.DispatchAsync(new RetrieveObjectCommand{Key = reducedQueueMessage.Message})))
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
                        var linesToWrite = (await _commandDispatcher.DispatchAsync(new FinalReducerFuncCommand{KeyValuePair = keyValuePair})).Result;
                        foreach (var lineToWrite in linesToWrite)
                            await streamWriter.WriteLineAsync(lineToWrite);
                    }

                    await streamWriter.FlushAsync();

                    var finalObjectKey = $"{_config.FinalReducedFolder}/{Guid.NewGuid()}";
                    await _commandDispatcher.DispatchAsync(new StoreObjectCommand
                    {
                        Key = finalObjectKey,
                        DataStream = memoryStream
                    });
                    await _queueClient.Enqueue(_config.FinalReducedQueueName, finalObjectKey);
                }
            }

            await _queueClient.MessageProcessed(_config.ReducedQueueName, reducedQueueMessage.MessageId);
            await _workerRecordStoreService.RecordHasTerminated("finalReducer", instanceWorkerId);
        }
    }
}