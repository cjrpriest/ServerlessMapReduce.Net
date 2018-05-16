using System;
using System.IO;
using Amazon.S3;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.AmazonS3
{
    class AmazonS3PermissionsProvider
    {
        private readonly IConfig _config;

        public AmazonS3PermissionsProvider(IConfig config)
        {
            _config = config;
        }
        
        public S3CannedACL GetPermissions(string key)
        {
            if (String.IsNullOrEmpty(key)) return S3CannedACL.NoACL;
            
            return key.StartsWith($"{_config.MonitoringFolder}{Path.DirectorySeparatorChar}")
                ? S3CannedACL.PublicRead
                : S3CannedACL.NoACL;
        }
    }
}