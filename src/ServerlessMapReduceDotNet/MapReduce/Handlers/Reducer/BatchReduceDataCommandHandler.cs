using System;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Commands.Map;
using ServerlessMapReduceDotNet.MapReduce.Commands.Reduce;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.MapReduce.Handlers.Reducer
{
    public class BatchReduceDataCommandHandler : ICommandHandler<BatchReduceDataCommand>
    {   
        private readonly IConfig _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandDispatcher _commandDispatcher;
        
        public BatchReduceDataCommandHandler(IConfig config, IServiceProvider serviceProvider, ICommandDispatcher commandDispatcher)
        {
            _config = config;
            _serviceProvider = serviceProvider;
            _commandDispatcher = commandDispatcher;
        }
        
        public async Task ExecuteAsync(BatchReduceDataCommand command)
        {
//            var reducerFunc = (IReducerFunc) _serviceProvider.GetService(_config.ReducerFuncType);
//
//            var keyValuePairCollection = reducerFunc.Reduce(command.InputKeyValuePairs);
//
//            await _commandDispatcher.DispatchAsync(new WriteReducedDataCommand
//            {
//                ReducedData = keyValuePairCollection,
//                ProcessedMessageIdsHash = command.ProcessedMessageIdsHash
//            });
        }
    }
}