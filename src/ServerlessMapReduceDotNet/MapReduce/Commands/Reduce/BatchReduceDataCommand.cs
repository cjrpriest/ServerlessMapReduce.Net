using System.Collections.Generic;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Commands.Reduce
{
    public class BatchReduceDataCommand : ICommand
    {
        public KeyValuePairCollection InputKeyValuePairs { get; set; }
        public List<CompressedMostAccidentProneData> InputKeyValuePairs2 { get; set; }
        public string ProcessedMessageIdsHash { get; set; }
        public string[] QueueMessages { get; set; }
    }

    public class CompressedMostAccidentProneData
    {
        public string M { get; set; }
        public CompressedAccidentStats S { get; set; }
    }

    public class CompressedAccidentStats
    {
        public int A { get; set; } //NoOfAccidents
        public int C { get; set; } //NoOfCarsRegistered
        public double R { get; set; } //RegistrationsPerAccident
    }
}