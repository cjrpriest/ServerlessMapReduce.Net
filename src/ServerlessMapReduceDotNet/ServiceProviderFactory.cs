using System;
using AzureFromTheTrenches.Commanding;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.FinalReducers;
using ServerlessMapReduceDotNet.Functions;
using ServerlessMapReduceDotNet.Handlers.Terminate;
using ServerlessMapReduceDotNet.HostingEnvironments;
using ServerlessMapReduceDotNet.LambdaEntryPoints;
using ServerlessMapReduceDotNet.Mappers;
using ServerlessMapReduceDotNet.MapReduce.Handlers;
using ServerlessMapReduceDotNet.MapReduce.Handlers.Monitoring;
using ServerlessMapReduceDotNet.ObjectStore.AmazonS3;
using ServerlessMapReduceDotNet.ObjectStore.FileSystem;
using ServerlessMapReduceDotNet.Queue.AmazonSqs;
using ServerlessMapReduceDotNet.Queue.InMemory;
using ServerlessMapReduceDotNet.Reducers;
using ServerlessMapReduceDotNet.Services;

namespace ServerlessMapReduceDotNet
{
    public class ServiceProviderFactory
    {
        public IServiceProvider Build(HostingEnvironment hostingEnvironment)
        {
            IServiceProvider serviceProvider = null;

            var serviceCollection = new ServiceCollection()

                .AddTransient<IFileObjectStoreConfig, LocalConfig>() //don't like this
                .AddTransient<AmazonS3PermissionsProvider>()

                .AddTransient<InMemoryQueueClient>()
                .AddTransient<AmazonSqsQueueClient>()

                .AddTransient<ITime, Time>()
                .AddTransient<IFileSystem, FileSystem>()

                .AddTransient<IWorkerRecordStoreService, WorkerRecordStoreService>()

                .AddTransient<MakeAccidentCountMapper>()
                .AddTransient<MostAccidentProneMapper>()
                
                .AddTransient<MakeAccidentCountReducer>()
                .AddTransient<MostAccidentProneReducer>()

                .AddTransient<MakeAccidentCountFinalReducer>()
                .AddTransient<MostAccidentProneFinalReducer>()
                
                .AddTransient<ICommandExecuter, AwsLambdaCommandExecuter>()
                .AddTransient<ICommandDispatcher, AwsLambdaCommandDispatcher>()

                .AddSingleton<IMemoryObjectStoreData, MemoryObjectStoreData>();

            var commandRegistry = new CommandingDependencyResolver(
                    (type, instance) => serviceCollection.AddSingleton(type, instance),
                    (type, impl) => serviceCollection.AddTransient(type, impl),
                    type => serviceProvider.GetService(type)
                )
                .UseCommanding()
                .Register<IsTerminatedCommandHandler>()
                .Register<UpdateMonitoringHandler>()
                .Register<MapperFuncHandler>()
                .Register<ReducerFuncHandler>()
                .Register<FinalReducerFuncHandler>();

            hostingEnvironment
                .RegisterHostingEnvironment(commandRegistry, serviceCollection, x =>
                {
                    x
                        .RegisterFireAndForgetFunctionImpl<Ingester, IngestCommand>()
                        .RegisterFireAndForgetFunctionImpl<Mapper, MapperCommand>()
                        .RegisterFireAndForgetFunctionImpl<Reducer, ReducerCommand>()
                        .RegisterFireAndForgetFunctionImpl<FinalReducer, FinalReducerCommand>()
                        .RegisterFireAndForgetFunctionImpl<WorkerManager, WorkerManagerCommand>();
                });
            
            serviceProvider = serviceCollection.BuildServiceProvider();

            return serviceProvider;
        }
    }
}