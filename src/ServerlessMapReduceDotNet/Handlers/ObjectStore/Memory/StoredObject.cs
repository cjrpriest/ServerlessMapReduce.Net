using System;

namespace ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory
{
    class StoredObject
    {
        public DateTime LastModified { get; set; }
        public byte[] Data { get; set; }
    }
}