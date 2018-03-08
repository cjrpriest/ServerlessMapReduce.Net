using System;

namespace ServerlessMapReduceDotNet.Abstractions
{
    public interface IConfig
    {
        string RawFolder { get; }
        string IngestedFolder { get; }
        string MappedFolder { get; }
        string ReducedFolder { get; }
        string WorkerRecordFolder { get; }
        string FinalReducedFolder { get; }
        string MonitoringFolder { get; }

        string RawDataQueueName { get; }
        string IngestedQueueName { get; }
        string MappedQueueName { get; }
        string ReducedQueueName { get; }
        string FinalReducedQueueName { get; }
        
        int IngesterMaxLinesPerFile { get; }
        
        Type MapperFuncType { get; }
        
        string AmazonSqsBaseUrl { get; }
        string AmazonS3BucketName { get; }
        
        int QueueItemsPerRunningWorker { get; }
        int MaxQueueItemsBatchSizeToProcessPerWorker { get; }
        int MaxNoOfRunningWorkerInstancesPerType { get; }
        int SleepBetweenWorkerManagerIterationsInMs { get; }
    }
}