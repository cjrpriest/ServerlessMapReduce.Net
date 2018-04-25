using System.IO;
using AzureFromTheTrenches.Commanding.Abstractions;

namespace ServerlessMapReduceDotNet.Commands.ObjectStore
{
    class StoreObjectCommand : ICommand
    {
        public string Key { get; set; }
        public Stream DataStream { get; set; }
    }
}