using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.ObjectStore.AmazonS3;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.Handlers.ObjectStore.AmazonS3
{    
    class AmazonS3RetrieveObjectCommandHandler : ICommandHandler<RetrieveObjectCommand, Stream>
    {
     private readonly IConfig _config;
     private readonly AmazonS3PermissionsProvider _permissionsProvider;
     private readonly AmazonS3Client _client = new AmazonS3Client(RegionEndpoint.EUWest1);
    
     public AmazonS3RetrieveObjectCommandHandler(IConfig config, AmazonS3PermissionsProvider permissionsProvider)
     {
         _config = config;
         _permissionsProvider = permissionsProvider;
     }
     
     public async Task<Stream> ExecuteAsync(RetrieveObjectCommand command, Stream previousResult)
     {
         Console.WriteLine($"Starting retrival of {command.Key}");
         var getObjectRequest = new GetObjectRequest
         {
             BucketName = _config.AmazonS3BucketName,
             Key = command.Key
         };
         var getObjectResponse = await _client.GetObjectAsync(getObjectRequest);
         if (getObjectResponse.HttpStatusCode != HttpStatusCode.OK)
             throw new ApplicationException($"Get of {_config.AmazonS3BucketName}/{command.Key} failed");
         Console.WriteLine($"Completed retrival of {command.Key}");
         return getObjectResponse.ResponseStream;
     }
    }
}