using System;
using System.IO;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.ObjectStore.FileSystem;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.FileSystem
{    
    class FileSystemStoreObjectCommandHandler : ICommandHandler<StoreObjectCommand>
    {
        private readonly IFileObjectStoreConfig _config;
        private readonly ITime _time;

        private static readonly object WriteLock = new object();
        
        public FileSystemStoreObjectCommandHandler(IFileObjectStoreConfig config, ITime time)
        {
            _config = config;
            _time = time;
        }
        
        public async Task ExecuteAsync(StoreObjectCommand command)
        {
            Console.WriteLine($"Storing stream to {command.Key}");

            var destinationPath = Path.Combine(_config.RootFileObjectStore, command.Key);
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            await Task.Run(() =>
            {
                var filePath = Path.Combine(_config.RootFileObjectStore, command.Key);
                lock (WriteLock) // crude locking system to prevent threads tripping over each other as they write to the same file
                {
                    using (var fileStream = File.Create(filePath, 4096, FileOptions.WriteThrough))
                    {
                        command.DataStream.Position = 0;
                        command.DataStream.CopyTo(fileStream);
                        fileStream.Flush(flushToDisk: true);
                    }
                    File.SetLastWriteTimeUtc(filePath, _time.UtcNow.ToLocalTime());
                }
            });
        }
    }
}