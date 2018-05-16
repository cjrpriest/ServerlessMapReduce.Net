using System;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.FileSystem;

namespace ServerlessMapReduceDotNet.Configuration
{
    public class FileSystemObjectStoreLocalConfig : IFileObjectStoreConfig
    {
        public string RootFileObjectStore => $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/FileObjectStore/";
    }
}