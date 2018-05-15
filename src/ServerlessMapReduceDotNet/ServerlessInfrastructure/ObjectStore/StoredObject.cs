using System;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore
{
    class StoredObject
    {
        public DateTime LastModified { get; set; }
        public byte[] Data { get; set; }
    }
}