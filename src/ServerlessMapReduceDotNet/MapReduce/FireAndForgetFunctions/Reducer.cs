using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Newtonsoft.Json;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Commands;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.Queue;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.MapReduce.FireAndForgetFunctions
{
    public class Reducer : IReducer
    {
        private readonly Regex _keyRegex = new Regex(@".*/(?<objectName>.*?)$", RegexOptions.Compiled);
        
        private readonly IQueueClient _queueClient;
        private readonly IConfig _config;
        private readonly IWorkerRecordStoreService _workerRecordStoreService;
        private readonly ICommandDispatcher _commandDispatcher;

        public Reducer(IQueueClient queueClient, IConfig config, IWorkerRecordStoreService workerRecordStoreService, ICommandDispatcher commandDispatcher)
        {
            _queueClient = queueClient;
            _config = config;
            _workerRecordStoreService = workerRecordStoreService;
            _commandDispatcher = commandDispatcher;
        }

        public async Task InvokeAsync()
        {
            var instanceWorkerId = _workerRecordStoreService.GenerateUniqueId();
            await _workerRecordStoreService.RecordPing("reducer", instanceWorkerId);
            
            var mappedQueueMessages = await _queueClient.Dequeue(_config.MappedQueueName, _config.MaxQueueItemsBatchSizeToProcessPerWorker);
            var reducedQueueMessages = await _queueClient.Dequeue(_config.ReducedQueueName, _config.MaxQueueItemsBatchSizeToProcessPerWorker);
            
            if (mappedQueueMessages.Count == 0 && reducedQueueMessages.Count <= 1)
            {
                foreach (var reducedQueueMessage in reducedQueueMessages)
                    await _queueClient.ReturnMessageToQueue(_config.ReducedQueueName, reducedQueueMessage.MessageId);
                await _workerRecordStoreService.RecordHasTerminated("reducer", instanceWorkerId);
                return;
            }

            var queueMessages = new List<QueueMessage>();
            queueMessages.AddRange(mappedQueueMessages);
            queueMessages.AddRange(reducedQueueMessages);
            
            using (var reducedDataMemoryStream = new MemoryStream())
            using (var reducedDataStreamWriter = new StreamWriter(reducedDataMemoryStream))
            {
                var inputCounts = new KeyValuePairCollection();

                foreach (var queueMessage in queueMessages)
                {
                    Stream mappedObjectStream = await _commandDispatcher.DispatchAsync(new RetrieveObjectCommand{Key = queueMessage.Message});
                    using (var streamReader = new StreamReader(mappedObjectStream))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            var line = await streamReader.ReadLineAsync();
                            try
                            {
                                var keyValuePairs = JsonConvert.DeserializeObject<KeyValuePairCollection>(line,
                                    new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

                                foreach (var keyValuePair in keyValuePairs)
                                {
                                    if (keyValuePair.GetType() == typeof(CountKvp))
                                        inputCounts.Add((CountKvp) keyValuePair);
                                    if (keyValuePair.GetType() == typeof(MostAccidentProneKvp))
                                        inputCounts.Add((MostAccidentProneKvp) keyValuePair);
                                }
                            }
                            catch (JsonSerializationException e)
                            {
                                Console.WriteLine($"Error white deserialising value [{line}]");
                            }
                        }
                    }
                }

                KeyValuePairCollection reducedCounts = await _commandDispatcher.DispatchAsync(new ReducerFuncCommand { InputKeyValuePairs= inputCounts});
                
                var reducedCountsJson = JsonConvert.SerializeObject(reducedCounts, Formatting.None,
                    new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});
                await reducedDataStreamWriter.WriteLineAsync(reducedCountsJson);

                await reducedDataStreamWriter.FlushAsync();
                var reducedOutputKey = $"{_config.ReducedFolder}/{Guid.NewGuid()}";

                // Without this being in a transaction, there is the risk of incorrect results
                MarkProcessed(_config.MappedQueueName, mappedQueueMessages);
                MarkProcessed(_config.ReducedQueueName, reducedQueueMessages);
                await _commandDispatcher.DispatchAsync(new StoreObjectCommand
                {
                    Key = reducedOutputKey,
                    DataStream = reducedDataMemoryStream
                });
                await _queueClient.Enqueue(_config.ReducedQueueName, reducedOutputKey);
                await _workerRecordStoreService.RecordHasTerminated("reducer", instanceWorkerId);
                // Transaction end
            }
        }

        private void MarkProcessed(string queueName, IList<QueueMessage> queueMessages)
        {
            foreach (var queueMessage in queueMessages)
            {
                _queueClient.MessageProcessed(queueName, queueMessage.MessageId).Wait();
            }
        }
    }
}