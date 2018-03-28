using System;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Handlers
{
    public class MapperFuncHandler : ICommandHandler<MapperFuncCommand, KeyValuePairCollection>
    {
        private readonly IConfig _config;
        private readonly IServiceProvider _serviceProvider;

        public MapperFuncHandler(IConfig config, IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }
        
        public async Task<KeyValuePairCollection> ExecuteAsync(MapperFuncCommand command, KeyValuePairCollection previousResult)
        {
            var mapperFunc = (IMapperFunc)_serviceProvider.GetService(_config.MapperFuncType);

            var keyValuePairCollection = mapperFunc.Map(command.Line);

            return keyValuePairCollection;
        }
    }
}