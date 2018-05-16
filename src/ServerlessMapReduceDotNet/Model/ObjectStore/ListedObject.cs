using System;

namespace ServerlessMapReduceDotNet.Model.ObjectStore
{
    public class ListedObject
    {
        public DateTime LastModified { get; set; }
        public string Key { get; set; }
    }
}