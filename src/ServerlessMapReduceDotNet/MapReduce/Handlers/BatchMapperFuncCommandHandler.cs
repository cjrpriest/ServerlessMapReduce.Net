using System;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Handlers
{
    public class BatchMapperFuncCommandHandler : ICommandHandler<BatchMapperFuncCommand>
    {
        private readonly IConfig _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandDispatcher _commandDispatcher;

        public BatchMapperFuncCommandHandler(IConfig config, IServiceProvider serviceProvider, ICommandDispatcher commandDispatcher)
        {
            _config = config;
            _serviceProvider = serviceProvider;
            _commandDispatcher = commandDispatcher;
        }
        
        public async Task ExecuteAsync(BatchMapperFuncCommand command)
        {
            var mapperFunc = (IMapperFunc)_serviceProvider.GetService(_config.MapperFuncType);
            
            var keyValuePairCollection = new KeyValuePairCollection();

            foreach (var line in command.Lines)
            {
                keyValuePairCollection.AddRange(mapperFunc.Map(line));
            }

            await _commandDispatcher.DispatchAsync(new WriteMapperResultsCommand
            {
                ContextQueueMessage = command.ContextQueueMessage,
                ResultOfMap = keyValuePairCollection
            });
        }
    }
}