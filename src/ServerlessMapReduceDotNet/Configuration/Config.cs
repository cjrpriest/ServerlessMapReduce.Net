using System;
using ServerlessMapReduceDotNet.MapReduce.Functions.MostAccidentProne;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.Configuration
{
    internal class Config : IConfig
    {
        public string WorkerRecordFolder => "workerRecord";
        public string FinalReducedFolder => "finalReduction";
        public string MonitoringFolder => "monitoring";
        public string RawFolder => "raw";
        public string IngestedFolder => "ingested";
        public string MappedFolder => "mapped";
        public string ReducedFolder => "reduced";

        public string RawDataQueueName => "serverless-mapreduce-rawdata.fifo";
        public string IngestedQueueName => "serverless-mapreduce-ingested.fifo";
        public string MappedQueueName => "serverless-mapreduce-mapped.fifo";
        public string ReducedQueueName => "serverless-mapreduce-reduced.fifo";
        public string FinalReducedQueueName => "serverless-mapreduce-finalreduced.fifo";

        public int IngesterMaxLinesPerFile => 10000;
        public Type MapperFuncType => typeof(MostAccidentProneMapper);
        public Type ReducerFuncType => typeof(MostAccidentProneReducer);
        public Type FinalReducerFuncType => typeof(MostAccidentProneFinalReducer);

        public string AmazonSqsBaseUrl => "https://sqs.eu-west-1.amazonaws.com/525470265062/";
        public string AmazonS3BucketName => "serverless-mapreduce";
        public int QueueItemsPerRunningWorker => 10;
        public int MaxQueueItemsBatchSizeToProcessPerWorker => 10;
        public int MaxNoOfRunningWorkerInstancesPerType => 10;
        public int SleepBetweenWorkerManagerIterationsInMs => 5000;
    }
}