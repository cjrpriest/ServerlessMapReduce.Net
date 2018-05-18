using System;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Commands.Map;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.MapReduce.Handlers.Mapper
{
    public class MapperFuncCommandHandler : ICommandHandler<MapperFuncCommand, KeyValuePairCollection>
    {
        private readonly IConfig _config;
        private readonly IServiceProvider _serviceProvider;

        public MapperFuncCommandHandler(IConfig config, IServiceProvider serviceProvider)
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