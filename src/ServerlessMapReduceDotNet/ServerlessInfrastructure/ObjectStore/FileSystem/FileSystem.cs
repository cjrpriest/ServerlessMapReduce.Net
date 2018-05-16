using System.IO;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.FileSystem
{
    class FileSystem : IFileSystem
    {
        public string[] Directory_GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFiles(path, searchPattern, searchOption);
        }
    }
}