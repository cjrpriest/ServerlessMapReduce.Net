using System.IO;

namespace ServerlessMapReduceDotNet.ObjectStore.FileSystem
{
    class FileSystem : IFileSystem
    {
        public string[] Directory_GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFiles(path, searchPattern, searchOption);
        }
    }
}