﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;

namespace ServerlessMapReduceDotNet.MapReduce.Handlers
{
    public class FinalReducerFuncHandler : ICommandHandler<FinalReducerFuncCommand, IReadOnlyCollection<string>>
    {
        private readonly IConfig _config;
        private readonly IServiceProvider _serviceProvider;

        public FinalReducerFuncHandler(IConfig config, IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }
        
        public async Task<IReadOnlyCollection<string>> ExecuteAsync(FinalReducerFuncCommand command, IReadOnlyCollection<string> previousResult)
        {
            var finalReducerFunc = (IFinalReduceFunc) _serviceProvider.GetService(_config.FinalReducerFuncType);

            var lines = finalReducerFunc.FinalReduce(command.KeyValuePair);

            return lines;
        }
    }
}