using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ServerlessMapReduceDotNet.Abstractions;

namespace ServerlessMapReduceDotNet.ObjectStore.FileSystem
{
    public class FileSystemObjectStore : IObjectStore
    {
        private readonly IFileSystem _fs;
        private readonly IFileObjectStoreConfig _config;
        private readonly ITime _time;
        
        private static readonly object _writeLock = new object();

        private readonly Regex[] _fileSystemEntriesToIgnoreRegexes = {
            new Regex(@".*?/.DS_Store$", RegexOptions.Compiled)
        };

        public FileSystemObjectStore(IFileSystem fileSystem, IFileObjectStoreConfig config, ITime time)
        {
            _fs = fileSystem;
            _config = config;
            _time = time;
        }
        
        public async Task StoreAsync(string key, Stream dataStream)
        {
            Console.WriteLine($"Storing stream to {key}");

            var destinationPath = Path.Combine(_config.RootFileObjectStore, key);
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            await Task.Run(() =>
            {
                var filePath = Path.Combine(_config.RootFileObjectStore, key);
                lock (_writeLock) // crude locking system to prevent threads tripping over each other as they write to the same file
                {
                    using (var fileStream = File.Create(filePath, 4096, FileOptions.WriteThrough))
                    {
                        dataStream.Position = 0;
                        dataStream.CopyTo(fileStream);
                        fileStream.Flush(flushToDisk: true);
                    }
                    File.SetLastWriteTimeUtc(filePath, _time.UtcNow.ToLocalTime());
                }
            });
        }

        public async Task<Stream> RetrieveAsync(string key)
        {
            Console.WriteLine($"Retrieving stream from {key}");
            
            return await Task.Run(() =>
            {
                Stream fileStream;
                try
                {
                    fileStream = File.Open(Path.Combine(_config.RootFileObjectStore, key), FileMode.Open);
                }
                catch (FileNotFoundException e)
                {
                    throw new InvalidOperationException($"Object stored in key [{key}] could not be found");
                } 
                return Task.FromResult(fileStream);
            });
        }

        public Task<IReadOnlyCollection<ListedObject>> ListKeysPrefixedAsync(string prefix)
        {
            var listedObjects = new List<ListedObject>();

            var pathToSearch = _config.RootFileObjectStore;
            if (pathToSearch.EndsWith(Path.DirectorySeparatorChar))
                pathToSearch = pathToSearch.Substring(0, pathToSearch.Length - 1);
            
            var fileSystemEntries = _fs.Directory_GetFiles(pathToSearch, "*", SearchOption.AllDirectories);

            var fileSytemEntriesToList = new List<string>();
            foreach (var fileSystemEntry in fileSystemEntries)
            {
                if (!_fileSystemEntriesToIgnoreRegexes.Any(x => x.Match(fileSystemEntry).Success))
                    fileSytemEntriesToList.Add(fileSystemEntry);
            }
            
            foreach (var filePath in fileSytemEntriesToList)
            {
                var relativePath = filePath.Replace($"{pathToSearch}{Path.DirectorySeparatorChar}", String.Empty);
                if (relativePath.StartsWith(prefix))
                {
                    var fileSystemInfo = new FileInfo(filePath);
                    listedObjects.Add(new ListedObject
                    {
                        Key = relativePath,
                        LastModified = fileSystemInfo.LastWriteTimeUtc
                    });
                }
            }
            
            return Task.FromResult<IReadOnlyCollection<ListedObject>>(listedObjects);
        }
    }
}