using NSubstitute;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.Tests.Builders
{
    class ConfigBuiler
    {
        private readonly IConfig _config = Substitute.For<IConfig>();

        public ConfigBuiler()
        {
            _config.MappedFolder.Returns("mapped");
            _config.WorkerRecordFolder.Returns("workerRecord");
            _config.MonitoringFolder.Returns("monitoringFolder");
            
            _config.RawDataQueueName.Returns("rawdata-queue");
            _config.IngestedQueueName.Returns("ingested-queue");
            _config.MappedQueueName.Returns("mapped-queue");
            _config.ReducedQueueName.Returns("reduced-queue");
            _config.FinalReducedQueueName.Returns("finalreduced-queue");
            _config.QueueItemsPerRunningWorker.Returns(10);
            _config.MaxQueueItemsBatchSizeToProcessPerWorker.Returns(10);
            _config.MaxNoOfRunningWorkerInstancesPerType.Returns(10);
        }
        
        public ConfigBuiler With(
            string rawDataQueueName = null,
            string ingestedQueueName = null,
            string reducedQueueName = null,
            int? queueItemsPerRunningWorker = null
            )
        {
            if (rawDataQueueName != null)
                _config.RawDataQueueName.Returns(rawDataQueueName);

            if (ingestedQueueName != null)
                _config.IngestedQueueName.Returns(ingestedQueueName);

            if (reducedQueueName != null)
                _config.ReducedQueueName.Returns(reducedQueueName);

            if (queueItemsPerRunningWorker != null)
                _config.QueueItemsPerRunningWorker.Returns(queueItemsPerRunningWorker.Value);
            
            return this;
        }
        
        internal IConfig Build()
        {
            return _config;
        }
    }
}