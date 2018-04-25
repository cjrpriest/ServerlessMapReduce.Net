using System.IO;
using AzureFromTheTrenches.Commanding.Abstractions;

namespace ServerlessMapReduceDotNet.Commands.ObjectStore
{
    class RetrieveObjectCommand : ICommand<Stream>
    {
        public string Key { get; set; }
    }
}