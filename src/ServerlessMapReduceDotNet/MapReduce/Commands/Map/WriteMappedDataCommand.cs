using System.Collections.Generic;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Commands.Reduce;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands.Map
{
    public class WriteMappedDataCommand : ICommand
    {
        public List<CompressedMostAccidentProneData> ResultOfMap2 { get; set; }
        public QueueMessage ContextQueueMessage { get; set; }
    }
}