using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Commands.Map;
using ServerlessMapReduceDotNet.MapReduce.Commands.Reduce;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.MapReduce.Handlers.Mapper
{
    public class BatchMapDataCommandHandler : ICommandHandler<BatchMapDataCommand>
    {
        private readonly IConfig _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandDispatcher _commandDispatcher;

        public BatchMapDataCommandHandler(IConfig config, IServiceProvider serviceProvider, ICommandDispatcher commandDispatcher)
        {
            _config = config;
            _serviceProvider = serviceProvider;
            _commandDispatcher = commandDispatcher;
        }
        
        public async Task ExecuteAsync(BatchMapDataCommand command)
        {
            var mapperFunc = (IMapperFunc)_serviceProvider.GetService(_config.MapperFuncType);
            
            var keyValuePairCollection = new KeyValuePairCollection();

            foreach (var line in command.Lines)
            {
                keyValuePairCollection.AddRange(mapperFunc.Map(line));
            }

            var resultOfMap2 = new List<CompressedMostAccidentProneData>();

            foreach (var kvp in keyValuePairCollection)
            {
                var mostAccidentProneKvp = kvp as MostAccidentProneKvp;
                resultOfMap2.Add(new CompressedMostAccidentProneData
                {
                    M = mostAccidentProneKvp.Key,
                    S = new CompressedAccidentStats
                    {
                        A = mostAccidentProneKvp.Value.NoOfAccidents,
                        C = mostAccidentProneKvp.Value.NoOfCarsRegistered,
                        R = mostAccidentProneKvp.Value.RegistrationsPerAccident
                    }
                });
            }

            await _commandDispatcher.DispatchAsync(new WriteMappedDataCommand
            {
                ContextQueueMessage = command.ContextQueueMessage,
//                ResultOfMap = keyValuePairCollection,
                ResultOfMap2 = resultOfMap2
            });
        }
    }
}