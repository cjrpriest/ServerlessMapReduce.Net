using System;

namespace ServerlessMapReduceDotNet.Model
{
    public class WorkerRecord
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public DateTime LastPingTime { get; set; }
        public bool ShouldRun { get; set; }
        public bool HasTerminated { get; set; }
    }
}