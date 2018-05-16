using System;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Queue.InMemory
{
    internal class InMemoryInternalQueueMessage
    {
        public string MessageId { get; set; }
        public int SequenceNumber { get; set; }
        public string Message { get; set; }
        public bool IsHidden { get; set; }
        public DateTime TimeHidden { get; set; }
        public TimeSpan VisibilityPeriod { get; set; }
    }
}