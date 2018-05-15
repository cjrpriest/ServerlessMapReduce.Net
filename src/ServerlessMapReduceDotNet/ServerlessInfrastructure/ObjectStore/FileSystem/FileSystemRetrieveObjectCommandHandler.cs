using System;
using System.IO;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.ObjectStore.FileSystem;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.FileSystem
{
    class FileSystemRetrieveObjectCommandHandler : ICommandHandler<RetrieveObjectCommand, Stream>
    {
        private readonly IFileObjectStoreConfig _config;

        public FileSystemRetrieveObjectCommandHandler(IFileObjectStoreConfig config)
        {
            _config = config;
        }
        
        public async Task<Stream> ExecuteAsync(RetrieveObjectCommand command, Stream previousResult)
        {
            Console.WriteLine($"Retrieving stream from {command.Key}");
            
            return await Task.Run(() =>
            {
                Stream fileStream;
                try
                {
                    fileStream = File.Open(Path.Combine(_config.RootFileObjectStore, command.Key), FileMode.Open);
                }
                catch (FileNotFoundException e)
                {
                    throw new InvalidOperationException($"Object stored in key [{command.Key}] could not be found");
                } 
                return Task.FromResult(fileStream);
            });
        }
    }
}