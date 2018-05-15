using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Newtonsoft.Json;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Functions
{    
    public class Mapper : IMapper
    {
        private readonly Regex _keyRegex = new Regex(@".*/(?<objectName>.*?)$", RegexOptions.Compiled);
        
        private readonly IQueueClient _queueClient;
        private readonly IConfig _config;
        private readonly IWorkerRecordStoreService _workerRecordStoreService;
        private readonly ICommandDispatcher _commandDispatcher;

        public Mapper(IQueueClient queueClient, IConfig config, IWorkerRecordStoreService workerRecordStoreService, ICommandDispatcher dispatch)
        {
            _queueClient = queueClient;
            _config = config;
            _workerRecordStoreService = workerRecordStoreService;
            _commandDispatcher = dispatch;
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

                Stream ingestedObjectStream = await _commandDispatcher.DispatchAsync(new RetrieveObjectCommand{Key = ingestedQueueMessage.Message});

                using (var streamReader = new StreamReader(ingestedObjectStream))
                {
                    var lines = new List<string>();
                    while (!streamReader.EndOfStream)
                    {
                        var line = await streamReader.ReadLineAsync();
                        lines.Add(line);
                    }

                    await _commandDispatcher.DispatchAsync(new BatchMapperFuncCommand
                    {
                        Lines = lines,
                        IngestedDataObjectName = ingestedDataObjectName,
                        IngestedQueueMessageId = ingestedQueueMessage.MessageId
                    });
                }
                
//                using (var memoryStream = new MemoryStream())
//                using (var streamWriter = new StreamWriter(memoryStream))
//                using (var streamReader = new StreamReader(ingestedObjectStream))
//                {
//                    while (!streamReader.EndOfStream)
//                    {
//                        var line = await streamReader.ReadLineAsync();
//                        
//                        try
//                        {
//                            KeyValuePairCollection keyValuePairs = await _commandDispatcher.DispatchAsync(new MapperFuncCommand {Line = line});
//                            if (keyValuePairs.Count > 0)
//                            {
//                                var keyValuePairsJson = JsonConvert.SerializeObject(keyValuePairs, Formatting.None,
//                                    new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});
//                                await streamWriter.WriteLineAsync(keyValuePairsJson);
//                            }
//                        }
//                        catch (Exception e)
//                        {
//                            Console.WriteLine($"Error while processing line \"{line}\"");
//                            Console.WriteLine(e.Demystify());
//                        }
//                    }
//
//                    await streamWriter.FlushAsync();
//                    var mapperOutputKey = $"{_config.MappedFolder}/{ingestedDataObjectName}";
//                    await _commandDispatcher.DispatchAsync(new StoreObjectCommand
//                    {
//                        Key = mapperOutputKey,
//                        DataStream = memoryStream
//                    });
//                    await _queueClient.Enqueue(_config.MappedQueueName, mapperOutputKey);
//
//                    await _queueClient.MessageProcessed(_config.IngestedQueueName, ingestedQueueMessage.MessageId);
//                }
            }

            await _workerRecordStoreService.RecordHasTerminated("mapper", instanceWorkerId);
            Console.WriteLine($"Mapper {instanceWorkerId} Terminated");
        }
    }
}