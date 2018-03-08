using System.IO;

namespace ServerlessMapReduceDotNet.ObjectStore.FileSystem
{
    public interface IFileSystem
    {
        string[] Directory_GetFiles(string path, string searchPattern, SearchOption searchOption);
    }
}