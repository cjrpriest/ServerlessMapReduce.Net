using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.Model.ObjectStore;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Helpers;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure
{
    public class WorkerRecordStoreService : IWorkerRecordStoreService
    {
        private const string LastPingTimePropertyName = "LastPingTime";
        private const string ShouldRunPropertyName = "ShouldRun";
        private const string HasTerminatedPropertyName = "HasTerminated";
        
        // /workerRecords/<workerType>/<workerId>/<propertyName>/<PropertyValue>
        private readonly Regex _workerRecordObjectKeyRegex = new Regex(@".*?/(?<WorkerType>.*?)/(?<WorkerId>.*?)/(?<PropertyName>.*?)/(?<PropertyValue>.*?)$", RegexOptions.Compiled);
        
        private readonly ITime _time;
        private readonly IConfig _config;
        private readonly ICommandDispatcher _commandDispatcher;

        public WorkerRecordStoreService(ITime time, IConfig config, ICommandDispatcher commandDispatcher)
        {
            _time = time;
            _config = config;
            _commandDispatcher = commandDispatcher;
        }

        public async Task<IReadOnlyCollection<WorkerRecord>> GetAllWorkerRecords()
        {
            var workerRecordsToReturn = new List<WorkerRecord>();
            IReadOnlyCollection<ListedObject> objectList =
                (await _commandDispatcher.DispatchAsync(new ListObjectKeysCommand
                {
                    Prefix = $"{_config.WorkerRecordFolder}/"
                })).Result;
            
            var workerProperties = objectList.SelectMany(GetWorkerRecordPropertyData);

            var typeIdNameGroups = workerProperties.GroupBy(x => $"{x.WorkerType}-{x.WorkerId}-{x.PropertyName}");

            foreach (var typeIdNameGroup in typeIdNameGroups)
            {
                var latestVersionOfProperty = FindLatestVersion(typeIdNameGroup.ToList());

                if (ThereAreNoWorkerRecordsForId(workerRecordsToReturn, latestVersionOfProperty.WorkerId))
                {
                    workerRecordsToReturn.Add(new WorkerRecord
                    {
                        Id = latestVersionOfProperty.WorkerId,
                        Type = latestVersionOfProperty.WorkerType,
                        ShouldRun = true,
                        HasTerminated = false,
                        LastPingTime = DateTime.MaxValue
                    });
                }

                var workerRecord = workerRecordsToReturn.First(x => latestVersionOfProperty.WorkerId == x.Id);

                UpdateWorkerRecordWithProperty(workerRecord, latestVersionOfProperty);
            }

            return workerRecordsToReturn;
        }

        private void UpdateWorkerRecordWithProperty(WorkerRecord workerRecord, WorkerRecordProperty latestVersionOfProperty)
        {
            switch (latestVersionOfProperty.PropertyName)
            {
                    case LastPingTimePropertyName:
                        workerRecord.LastPingTime = DateTimeOffset
                            .FromUnixTimeSeconds(long.Parse(latestVersionOfProperty.PropertyValue)).DateTime;
                        break;
                    case ShouldRunPropertyName:
                        workerRecord.ShouldRun = bool.Parse(latestVersionOfProperty.PropertyValue);
                        break;
                    case HasTerminatedPropertyName:
                        workerRecord.HasTerminated = bool.Parse(latestVersionOfProperty.PropertyValue);
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Property name {latestVersionOfProperty.PropertyName} is not recognised");
            }
        }

        private WorkerRecordProperty FindLatestVersion(IReadOnlyCollection<WorkerRecordProperty> typeIdNameGroup)
        {
            var propertyName = typeIdNameGroup.First().PropertyName;
            
            switch (propertyName)
            {
                case LastPingTimePropertyName:
                    return typeIdNameGroup
                        .OrderByDescending(x => x.LastModified)
                        .ThenByDescending(x => DateTimeOffset.FromUnixTimeSeconds(long.Parse(x.PropertyValue)).Date)
                        .First();
                case ShouldRunPropertyName:
                    return typeIdNameGroup
                        .OrderByDescending(x => x.LastModified)
                        .ThenBy(x => x.PropertyValue) // False should override True
                        .First();
                case HasTerminatedPropertyName:
                    return typeIdNameGroup
                        .OrderByDescending(x => x.LastModified)
                        .ThenByDescending(x => x.PropertyValue) // True should override False
                        .First();
                default:
                    throw new InvalidOperationException(
                        $"Property name {propertyName} is not recognised");
            }
        }

        private static bool ThereAreNoWorkerRecordsForId(List<WorkerRecord> workerRecords, string workerId)
        {
            return workerRecords.All(x => x.Id != workerId);
        }

        private IEnumerable<WorkerRecordProperty> GetWorkerRecordPropertyData(ListedObject obj)
        {
            var workerRecordPropertyData = new List<WorkerRecordProperty>();
            var regexMatch = _workerRecordObjectKeyRegex.Match(obj.Key);
            if (!regexMatch.Success)
                return workerRecordPropertyData;
            
            workerRecordPropertyData.Add(new WorkerRecordProperty
            {
                LastModified = obj.LastModified,
                WorkerType = regexMatch.Groups["WorkerType"].Value,
                WorkerId = regexMatch.Groups["WorkerId"].Value,
                PropertyName = regexMatch.Groups["PropertyName"].Value,
                PropertyValue = regexMatch.Groups["PropertyValue"].Value
            });
            
            return workerRecordPropertyData;
        }

        public async Task RecordPing(string workerType, string workerId)
        {
            var timeValue = GetLastPingTimeValue(_time.UtcNow);
            var objectKey =
                $"{_config.WorkerRecordFolder}/{workerType}/{workerId}/{LastPingTimePropertyName}/{timeValue}";
            await _commandDispatcher.DispatchAsync(new StoreObjectCommand
            {
                Key = objectKey,
                DataStream = StreamHelper.NewEmptyStream()
            });
        }

        public async Task RecordShouldStop(string workerType, string workerId)
        {
            var objectKey =
                $"{_config.WorkerRecordFolder}/{workerType}/{workerId}/{ShouldRunPropertyName}/False";
            await _commandDispatcher.DispatchAsync(new StoreObjectCommand
            {
                Key = objectKey,
                DataStream = StreamHelper.NewEmptyStream()
            });
        }

        public async Task RecordHasTerminated(string workerType, string workerId)
        {
            var objectKey =
                $"{_config.WorkerRecordFolder}/{workerType}/{workerId}/{HasTerminatedPropertyName}/True";
            await _commandDispatcher.DispatchAsync(new StoreObjectCommand
            {
                Key = objectKey,
                DataStream = StreamHelper.NewEmptyStream()
            });
        }

        private string GetLastPingTimeValue(DateTime timeUtcNow)
        {
            var unixTime = ((DateTimeOffset) timeUtcNow).ToUnixTimeSeconds();
            return unixTime.ToString();
        }

        private class WorkerRecordProperty
        {
            public DateTime LastModified { get; set; }
            public string WorkerType { get; set; }
            public string WorkerId { get; set; }
            public string PropertyName { get; set; }
            public string PropertyValue { get; set; }
        }
        
        public string GenerateUniqueId()
        {
            return UniqueIdHelper.GenerateUniqueId();
        }
    }
}