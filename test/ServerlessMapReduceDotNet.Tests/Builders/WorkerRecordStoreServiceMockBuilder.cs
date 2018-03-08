using System;
using System.Collections.Generic;
using NSubstitute;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.Services;

namespace ServerlessMapReduceDotNet.Tests.Builders
{
    public class WorkerRecordStoreServiceMockBuilder
    {
        private readonly IWorkerRecordStoreService _workerRecordStoreService = Substitute.For<IWorkerRecordStoreService>();
        private readonly List<WorkerRecord> _workerRecords = new List<WorkerRecord>();

        public WorkerRecordStoreServiceMockBuilder WithWorkerRecords(int noOfWorkerRecords, string type,
            DateTime lastPingTime = default(DateTime))
        {
            for (int i = 0; i < noOfWorkerRecords; i++)
                WithWorkerRecord(type, lastPingTime);

            return this;
        }
        
        public WorkerRecordStoreServiceMockBuilder WithWorkerRecord(string type, DateTime lastPingTime = default(DateTime))
        {
            if (lastPingTime == default(DateTime))
                lastPingTime = DateTime.UtcNow;
            
            _workerRecords.Add(new WorkerRecord
            {
                Id = UniqueIdHelper.GenerateUniqueId(),
                Type = type,
                HasTerminated = false,
                ShouldRun = true,
                LastPingTime = lastPingTime                
            });

            _workerRecordStoreService.GetAllWorkerRecords().Returns(_workerRecords);

            return this;
        }
        
        public IWorkerRecordStoreService Build()
        {
            return _workerRecordStoreService;
        }
    }
}