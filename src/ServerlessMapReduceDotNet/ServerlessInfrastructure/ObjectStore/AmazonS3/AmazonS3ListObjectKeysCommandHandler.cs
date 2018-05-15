using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.ObjectStore;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.AmazonS3
{
    class AmazonS3ListObjectKeysCommandHandler : ICommandHandler<ListObjectKeysCommand,IReadOnlyCollection<ListedObject>>
    {
        private readonly IConfig _config;
        private readonly AmazonS3Client _client = new AmazonS3Client(RegionEndpoint.EUWest1); 

        public AmazonS3ListObjectKeysCommandHandler(IConfig config)
        {
            _config = config;
        }

        public async Task<IReadOnlyCollection<ListedObject>> ExecuteAsync(ListObjectKeysCommand command, IReadOnlyCollection<ListedObject> previousResult)
        {
            Console.WriteLine($"Starting listing of prefix {command.Prefix}");
            var listedObjects = new List<ListedObject>();
            var listObjectsRequest = new ListObjectsV2Request
            {
                BucketName = _config.AmazonS3BucketName,
                Prefix = command.Prefix
            };
            ListObjectsV2Response listObjectsResponse;
            do
            {
                listObjectsResponse = await _client.ListObjectsV2Async(listObjectsRequest);
                var newListedObjects = listObjectsResponse.S3Objects.Select(x => new ListedObject
                {
                    Key = x.Key,
                    LastModified = x.LastModified
                });
                listedObjects.AddRange(newListedObjects);
                listObjectsRequest.ContinuationToken = listObjectsResponse.ContinuationToken;
            } while (listObjectsResponse.IsTruncated);

            Console.WriteLine($"Completed listing of prefix {command.Prefix}");
            return listedObjects;
        }
    }
}