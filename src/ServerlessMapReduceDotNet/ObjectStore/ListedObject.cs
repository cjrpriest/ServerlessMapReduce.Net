using System;

namespace ServerlessMapReduceDotNet.ObjectStore
{
    public class ListedObject
    {
        public DateTime LastModified { get; set; }
        public string Key { get; set; }
    }
}