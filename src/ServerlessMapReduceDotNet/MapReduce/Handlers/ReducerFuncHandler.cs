using System;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Commands;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.MapReduce.Handlers
{
    public class ReducerFuncHandler : ICommandHandler<ReducerFuncCommand, KeyValuePairCollection>
    {
        private readonly IConfig _config;
        private readonly IServiceProvider _serviceProvider;

        public ReducerFuncHandler(IConfig config, IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }

        public async Task<KeyValuePairCollection> ExecuteAsync(ReducerFuncCommand command, KeyValuePairCollection previousResult)
        {
            var reducerFunc = (IReducerFunc) _serviceProvider.GetService(_config.ReducerFuncType);

            var keyValuePairCollection = reducerFunc.Reduce(command.InputKeyValuePairs);

            return keyValuePairCollection;
        }
    }
}