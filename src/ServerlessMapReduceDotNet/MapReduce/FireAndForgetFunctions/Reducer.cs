using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Newtonsoft.Json;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Commands.Reduce;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Helpers;

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

            var inputCounts = new List<CompressedMostAccidentProneData>();
            
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
                            var keyValuePairs = JsonConvert.DeserializeObject<List<CompressedMostAccidentProneData>>(line,
                                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});
                            inputCounts.AddRange(keyValuePairs);
                        }
                        catch (JsonSerializationException e)
                        {
                            Console.WriteLine($"Error white deserialising value [{line}]");
                        }
                    }
                }
            }

            await _commandDispatcher.DispatchAsync(new BatchReduceDataCommand
            {
                InputKeyValuePairs2 = inputCounts,
                ProcessedMessageIdsHash = ProcessedMessageIdsHash(queueMessages),
                QueueMessages = queueMessages.Select(x => x.Message).ToArray()
            });
            
            // Without this being in a transaction, there is the risk of incorrect results
            MarkProcessed(_config.MappedQueueName, mappedQueueMessages);
            MarkProcessed(_config.ReducedQueueName, reducedQueueMessages);
            await _workerRecordStoreService.RecordHasTerminated("reducer", instanceWorkerId);
            // Transaction end
        }

        private void MarkProcessed(string queueName, IList<QueueMessage> queueMessages)
        {
            foreach (var queueMessage in queueMessages)
            {
                _queueClient.MessageProcessed(queueName, queueMessage.MessageId).Wait();
            }
        }

        private string ProcessedMessageIdsHash(IReadOnlyCollection<QueueMessage> messages)
        {
            var messageIds = new StringBuilder();
            
            foreach (var message in messages)
            {
                messageIds.Append(message.MessageId);
            }

            var hash = HashHelper.GetHashSha256(messageIds.ToString());

            return hash;
        }

        private string KeyValuePairCollectionHash(KeyValuePairCollection keyValuePairCollection)
        {
            var stringBuilder = new StringBuilder();

            foreach (var kvp in keyValuePairCollection)
            {
                stringBuilder.Append(JsonConvert.SerializeObject(kvp,
                    new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto}));
            }

            return HashHelper.GetHashSha256(stringBuilder.ToString());
        }
        
        private int CompareMostAccidentProneKvps(IKeyValuePair x, IKeyValuePair y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            
            var mostAccidentProneKvp1 = x as MostAccidentProneKvp;
            if (mostAccidentProneKvp1 == null) throw new ArgumentNullException(nameof(mostAccidentProneKvp1));

            var mostAccidentProneKvp2 = y as MostAccidentProneKvp;
            if (mostAccidentProneKvp2 == null) throw new ArgumentNullException(nameof(mostAccidentProneKvp2));
            
            if (mostAccidentProneKvp1.Value == null) throw new ArgumentNullException($"{nameof(mostAccidentProneKvp1)}.{nameof(mostAccidentProneKvp1.Value)}");
            if (mostAccidentProneKvp2.Value == null) throw new ArgumentNullException($"{nameof(mostAccidentProneKvp2)}.{nameof(mostAccidentProneKvp2.Value)}");

            return mostAccidentProneKvp2.Value.RegistrationsPerAccident.CompareTo(
                mostAccidentProneKvp1.Value.RegistrationsPerAccident);
        }
    }
}