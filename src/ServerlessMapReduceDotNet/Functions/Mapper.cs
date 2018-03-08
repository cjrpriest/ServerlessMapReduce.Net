using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Newtonsoft.Json;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;

namespace ServerlessMapReduceDotNet.Functions
{    
    public class Mapper : IMapper
    {
        private readonly Regex _keyRegex = new Regex(@".*/(?<objectName>.*?)$", RegexOptions.Compiled);
        
        private readonly IQueueClient _queueClient;
        private readonly IObjectStore _objectStore;
        private readonly IConfig _config;
        private readonly IWorkerRecordStoreService _workerRecordStoreService;
        private readonly ICommandDispatcher _commandDispatcher;

        public Mapper(IQueueClient queueClient, IObjectStore objectStore, IConfig config, IWorkerRecordStoreService workerRecordStoreService, ICommandDispatcher commandDispatcher)
        {
            _queueClient = queueClient;
            _objectStore = objectStore;
            _config = config;
            _workerRecordStoreService = workerRecordStoreService;
            _commandDispatcher = commandDispatcher;
        }

        public async Task InvokeAsync()
        {
            var instanceWorkerId = _workerRecordStoreService.GenerateUniqueId();
            Console.WriteLine($"Mapper {instanceWorkerId} starting...");
            await _workerRecordStoreService.RecordPing("mapper", instanceWorkerId);
            
            var ingestedQueueMessages = await _queueClient.Dequeue(_config.IngestedQueueName, _config.MaxQueueItemsBatchSizeToProcessPerWorker);
            if (ingestedQueueMessages.Count == 0)
            {
                await _workerRecordStoreService.RecordHasTerminated("mapper", instanceWorkerId);
                return;
            }
            
            foreach (var ingestedQueueMessage in ingestedQueueMessages)
            {
                await _workerRecordStoreService.RecordPing("mapper", instanceWorkerId);
                var ingestedDataObjectName = _keyRegex.Match(ingestedQueueMessage.Message).Groups["objectName"].Value;

                var ingestedObjectStream = await _objectStore.RetrieveAsync(ingestedQueueMessage.Message);

                using (var memoryStream = new MemoryStream())
                using (var streamWriter = new StreamWriter(memoryStream))
                using (var streamReader = new StreamReader(ingestedObjectStream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = await streamReader.ReadLineAsync();
                        try
                        {
                            var keyValuePairs = await _commandDispatcher.DispatchAsync(new MapperFuncCommand {Line = line});
                            if (keyValuePairs.Result.Count > 0)
                            {
                                var keyValuePairsJson = JsonConvert.SerializeObject(keyValuePairs.Result,
                                    Formatting.None,
                                    new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});
                                await streamWriter.WriteLineAsync(keyValuePairsJson);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error while processing line \"{line}\"");
                            Console.WriteLine(e.Demystify());
                        }
                    }

                    await streamWriter.FlushAsync();
                    var mapperOutputKey = $"{_config.MappedFolder}/{ingestedDataObjectName}";
                    await _objectStore.StoreAsync(mapperOutputKey, memoryStream);
                    await _queueClient.Enqueue(_config.MappedQueueName, mapperOutputKey);

                    await _queueClient.MessageProcessed(_config.IngestedQueueName, ingestedQueueMessage.MessageId);
                }
            }

            await _workerRecordStoreService.RecordHasTerminated("mapper", instanceWorkerId);
            Console.WriteLine($"Mapper {instanceWorkerId} Terminated");
        }
    }
}