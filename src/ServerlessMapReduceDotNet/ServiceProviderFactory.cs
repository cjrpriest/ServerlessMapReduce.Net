using System;
using AzureFromTheTrenches.Commanding;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.Configuration;
using ServerlessMapReduceDotNet.EntryPoints.Lambda;
using ServerlessMapReduceDotNet.HostingEnvironments;
using ServerlessMapReduceDotNet.MapReduce.Commands;
using ServerlessMapReduceDotNet.MapReduce.FireAndForgetFunctions;
using ServerlessMapReduceDotNet.MapReduce.Functions.MakeAccidentCount;
using ServerlessMapReduceDotNet.MapReduce.Functions.MostAccidentProne;
using ServerlessMapReduceDotNet.MapReduce.Handlers;
using ServerlessMapReduceDotNet.MapReduce.Handlers.Mapper;
using ServerlessMapReduceDotNet.MapReduce.Handlers.Monitoring;
using ServerlessMapReduceDotNet.Queue.InMemory;
using ServerlessMapReduceDotNet.ServerlessInfrastructure;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Handlers.Terminate;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.AmazonS3;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.FileSystem;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.Memory;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Queue.AmazonSqs;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Queue.InMemory;

namespace ServerlessMapReduceDotNet
{
    public class ServiceProviderFactory
    {
        public IServiceProvider Build(HostingEnvironment hostingEnvironment)
        {
            IServiceProvider serviceProvider = null;

            var serviceCollection = new ServiceCollection()

                .AddTransient<IFileObjectStoreConfig, FileSystemObjectStoreLocalConfig>() //don't like this
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

                .AddSingleton<IMemoryObjectStore, MemoryObjectStore>();

            var commandRegistry = new CommandingDependencyResolver(
                    (type, instance) => serviceCollection.AddSingleton(type, instance),
                    (type, impl) => serviceCollection.AddTransient(type, impl),
                    type => serviceProvider.GetService(type)
                )
                .UseCommanding()
                .Register<IsTerminatedCommandHandler>()
                .Register<UpdateMonitoringHandler>()
                .Register<MapperFuncCommandHandler>()
                .Register<ReducerFuncHandler>()
                .Register<FinalReducerFuncHandler>()

                .Register<BatchMapperFuncCommandHandler>()
                .Register<WriteMapperResultsCommandHandler>();

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