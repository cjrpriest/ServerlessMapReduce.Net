using System;
using ServerlessMapReduceDotNet.ObjectStore.FileSystem;

namespace ServerlessMapReduceDotNet
{
    public class LocalConfig : IFileObjectStoreConfig
    {
        public string RootFileObjectStore => $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/FileObjectStore/";
    }
}