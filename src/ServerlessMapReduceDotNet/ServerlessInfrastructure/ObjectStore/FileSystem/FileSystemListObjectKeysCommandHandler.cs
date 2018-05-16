using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.Model.ObjectStore;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.FileSystem
{
    class FileSystemListObjectKeysCommandHandler : ICommandHandler<ListObjectKeysCommand, IReadOnlyCollection<ListedObject>>
    {
        private readonly Regex[] _fileSystemEntriesToIgnoreRegexes = {
            new Regex(@".*?/.DS_Store$", RegexOptions.Compiled)
        };
        
        private readonly IFileObjectStoreConfig _config;
        private readonly IFileSystem _fs;

        public FileSystemListObjectKeysCommandHandler(IFileObjectStoreConfig config, IFileSystem fs)
        {
            _config = config;
            _fs = fs;
        }
        
        public Task<IReadOnlyCollection<ListedObject>> ExecuteAsync(ListObjectKeysCommand command, IReadOnlyCollection<ListedObject> previousResult)
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
                if (relativePath.StartsWith(command.Prefix))
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