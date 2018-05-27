using System;
using ServerlessMapReduceDotNet.MapReduce.Functions.MakeAccidentCount;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.Tests.IntegrationTests
{
    public class IntegrationTestConfig : IConfig
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
        public string CommandQueueName => "serverless-mapreduce-commandQueue.fifo";
        public string RemoteCommandQueueName  => "serverless-mapreduce-remoteCommandQueue.fifo";

        public int IngesterMaxLinesPerFile => 1000;
        public Type MapperFuncType => typeof(MakeAccidentCountMapper);
        public Type ReducerFuncType => typeof(MakeAccidentCountReducer);
        public Type FinalReducerFuncType => typeof(MakeAccidentCountFinalReducer);

        public string AmazonSqsBaseUrl => "https://sqs.eu-west-1.amazonaws.com/525470265062/";
        public string AmazonSqsServiceUrl => "https://sqs.eu-west-1.amazonaws.com/";
        public string AmazonS3BucketName => "serverless-mapreduce"; 
        public int QueueItemsPerRunningWorker => 10;
        public int MaxQueueItemsBatchSizeToProcessPerWorker => 10;
        public int MaxNoOfRunningWorkerInstancesPerType => 5;
        public int SleepBetweenWorkerManagerIterationsInMs => 250;
    }
}