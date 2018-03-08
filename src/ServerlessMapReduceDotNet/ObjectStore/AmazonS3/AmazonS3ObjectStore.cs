using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using ServerlessMapReduceDotNet.Abstractions;

namespace ServerlessMapReduceDotNet.ObjectStore.AmazonS3
{
    internal class AmazonS3ObjectStore : IObjectStore
    {
        private readonly IConfig _config;
        private readonly AmazonS3PermissionsProvider _permissionsProvider;
        private readonly AmazonS3Client _client = new AmazonS3Client(RegionEndpoint.EUWest1); 
        
        public AmazonS3ObjectStore(IConfig config, AmazonS3PermissionsProvider permissionsProvider)
        {
            _config = config;
            _permissionsProvider = permissionsProvider;
        }

        public async Task StoreAsync(string key, Stream dataStream)
        {
            Console.WriteLine($"Starting write to {key}");
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = _config.AmazonS3BucketName,
                Key = key,
                InputStream = dataStream,
                CannedACL = _permissionsProvider.GetPermissions(key)
            };
            var putObjectResponse = await _client.PutObjectAsync(putObjectRequest);
            if (putObjectResponse.HttpStatusCode != HttpStatusCode.OK)
                throw new ApplicationException($"Put of {_config.AmazonS3BucketName}/{key} failed");
            Console.WriteLine($"Completed write to {key}");
        }

        public async Task<Stream> RetrieveAsync(string key)
        {
            Console.WriteLine($"Starting retrival of {key}");
            var getObjectRequest = new GetObjectRequest
            {
                BucketName = _config.AmazonS3BucketName,
                Key = key
            };
            var getObjectResponse = await _client.GetObjectAsync(getObjectRequest);
            if (getObjectResponse.HttpStatusCode != HttpStatusCode.OK)
                throw new ApplicationException($"Get of {_config.AmazonS3BucketName}/{key} failed");
            Console.WriteLine($"Completed retrival of {key}");
            return getObjectResponse.ResponseStream;
        }

        public async Task<IReadOnlyCollection<ListedObject>> ListKeysPrefixedAsync(string prefix)
        {
            Console.WriteLine($"Starting listing of prefix {prefix}");
            var listedObjects = new List<ListedObject>();
            var listObjectsRequest = new ListObjectsV2Request
            {
                BucketName = _config.AmazonS3BucketName,
                Prefix = prefix
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

            Console.WriteLine($"Completed listing of prefix {prefix}");
            return listedObjects;
        }
    }
}