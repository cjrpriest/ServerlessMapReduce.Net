using System;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.FileSystem;

namespace ServerlessMapReduceDotNet
{
    public class LocalConfig : IFileObjectStoreConfig
    {
        public string RootFileObjectStore => $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/FileObjectStore/";
    }
}