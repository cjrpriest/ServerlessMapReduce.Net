using System;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.ObjectStore.AmazonS3;

namespace ServerlessMapReduceDotNet.Handlers.ObjectStore.AmazonS3
{
    class AmazonS3StoreObjectCommandHandler : ICommandHandler<StoreObjectCommand>
    {
        private readonly IConfig _config;
        private readonly AmazonS3PermissionsProvider _permissionsProvider;
        private readonly AmazonS3Client _client = new AmazonS3Client(RegionEndpoint.EUWest1); 

        public AmazonS3StoreObjectCommandHandler(IConfig config, AmazonS3PermissionsProvider permissionsProvider)
        {
            _config = config;
            _permissionsProvider = permissionsProvider;
        }
        
        public async Task ExecuteAsync(StoreObjectCommand command)
        {
            Console.WriteLine($"Starting write to {command.Key}");
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = _config.AmazonS3BucketName,
                Key = command.Key,
                InputStream = command.DataStream,
                CannedACL = _permissionsProvider.GetPermissions(command.Key)
            };
            var putObjectResponse = await _client.PutObjectAsync(putObjectRequest);
            if (putObjectResponse.HttpStatusCode != HttpStatusCode.OK)
                throw new ApplicationException($"Put of {_config.AmazonS3BucketName}/{command.Key} failed");
            Console.WriteLine($"Completed write to {command.Key}");
        }
    }
}