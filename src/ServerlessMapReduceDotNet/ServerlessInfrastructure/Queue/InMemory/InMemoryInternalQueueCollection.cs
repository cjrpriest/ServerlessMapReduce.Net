using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Queue.InMemory
{
    internal class InMemoryInternalQueueCollection : Dictionary<string, ConcurrentDictionary<string, InMemoryInternalQueueMessage>>
    {
        
    }
}