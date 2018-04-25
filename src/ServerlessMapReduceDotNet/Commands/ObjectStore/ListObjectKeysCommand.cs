using System.Collections.Generic;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.ObjectStore;

namespace ServerlessMapReduceDotNet.Commands.ObjectStore
{
    class ListObjectKeysCommand : ICommand<IReadOnlyCollection<ListedObject>>
    {
        public string Prefix { get; set; }
    }
}