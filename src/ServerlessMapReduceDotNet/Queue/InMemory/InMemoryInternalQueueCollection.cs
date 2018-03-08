using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ServerlessMapReduceDotNet.Queue.InMemory
{
    internal class InMemoryInternalQueueCollection : Dictionary<string, ConcurrentDictionary<string, InMemoryInternalQueueMessage>>
    {
        
    }
}