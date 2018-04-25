using System.IO;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;

namespace ServerlessMapReduceDotNet.Handlers.ObjectStore
{
    public static class ObjectStoreExtensions
    {
        public static async Task<string> GetObjectAsString(this ICommandDispatcher commandDispatcher, string key)
        {
            using (Stream stream = await commandDispatcher.DispatchAsync(new RetrieveObjectCommand{Key = key}))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}