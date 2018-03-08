using System;

namespace ServerlessMapReduceDotNet.ObjectStore
{
    public class StoredObject
    {
        public DateTime LastModified { get; set; }
        public byte[] Data { get; set; }
    }
}