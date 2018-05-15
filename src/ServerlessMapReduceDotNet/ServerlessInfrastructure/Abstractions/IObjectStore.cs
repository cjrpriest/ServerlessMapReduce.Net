using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ServerlessMapReduceDotNet.ObjectStore;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions
{
    public interface IObjectStore
    {
        Task StoreAsync(string key, Stream dataStream);
        Task<Stream> RetrieveAsync(string key);
        Task<IReadOnlyCollection<ListedObject>> ListKeysPrefixedAsync(string prefix);
    }
}