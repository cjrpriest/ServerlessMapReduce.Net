using System.Collections.Generic;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.Model.ObjectStore;

namespace ServerlessMapReduceDotNet.Commands.ObjectStore
{
    class ListObjectKeysCommand : ICommand<IReadOnlyCollection<ListedObject>>
    {
        public string Prefix { get; set; }
    }
}