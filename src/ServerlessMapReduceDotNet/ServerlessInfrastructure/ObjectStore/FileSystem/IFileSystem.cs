using System.IO;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.FileSystem
{
    public interface IFileSystem
    {
        string[] Directory_GetFiles(string path, string searchPattern, SearchOption searchOption);
    }
}