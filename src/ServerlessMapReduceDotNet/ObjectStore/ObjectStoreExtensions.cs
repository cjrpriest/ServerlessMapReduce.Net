using System.IO;
using ServerlessMapReduceDotNet.Abstractions;

namespace ServerlessMapReduceDotNet.ObjectStore
{
    public static class ObjectStoreExtensions
    {
        public static string GetObjectAsString(this IObjectStore objectStore, string area, string key)
        {
            using (var stream = objectStore.RetrieveAsync(key).Result)
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}