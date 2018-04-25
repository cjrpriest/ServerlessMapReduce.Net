﻿using System;
using AzureFromTheTrenches.Commanding;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.Functions;
using ServerlessMapReduceDotNet.HostingEnvironments;
using ServerlessMapReduceDotNet.LambdaEntryPoints;
using ServerlessMapReduceDotNet.Mappers;
using ServerlessMapReduceDotNet.MapReduce.Handlers;
using ServerlessMapReduceDotNet.MapReduce.Handlers.Monitoring;
using ServerlessMapReduceDotNet.ObjectStore.AmazonS3;
using ServerlessMapReduceDotNet.ObjectStore.FileSystem;
using ServerlessMapReduceDotNet.Queue.AmazonSqs;
using ServerlessMapReduceDotNet.Queue.InMemory;
using ServerlessMapReduceDotNet.Services;

namespace ServerlessMapReduceDotNet
{
    public class ServiceProviderFactory
    {
        public IServiceProvider Build(HostingEnvironment hostingEnvironment)
        {
            IServiceProvider serviceProvider = null;

            var serviceCollection = new ServiceCollection()
                
                .AddTransient<FileSystemObjectStore>()
                .AddTransient<IFileObjectStoreConfig, LocalConfig>() //don't like this
                .AddTransient<AmazonS3ObjectStore>()
                    .AddTransient<AmazonS3PermissionsProvider>()
                
                .AddTransient<InMemoryQueueClient>()
                .AddTransient<AmazonSqsQueueClient>()
                
                .AddTransient<ITime, Time>()
                .AddTransient<IFileSystem, FileSystem>()

                .AddTransient<IWorkerRecordStoreService, WorkerRecordStoreService>()
                
                .AddTransient<MakeAccidentCountMapper>()
                .AddTransient<MostAccidentProneMapper>()
                
                .AddTransient<ICommandExecuter, AwsLambdaCommandExecuter>()
                .AddTransient<ICommandDispatcher, AwsLambdaCommandDispatcher>()
                
                .AddSingleton<IMemoryObjectStoreData, MemoryObjectStoreData>()

                .RegisterHostingEnvironment(hostingEnvironment);

            new CommandingDependencyResolver(
                    (type, instance) => serviceCollection.AddSingleton(type, instance),
                    (type, impl) => serviceCollection.AddTransient(type, impl),
                    type => serviceProvider.GetService(type)
                )
                .UseCommanding()
                .Register<UpdateMonitoringHandler>()
                .Register<MapperFuncHandler>()
                .RegisterHostingEnvironment(hostingEnvironment);

            hostingEnvironment
                .RegisterFireAndForgetFunction<Ingester, IngestCommand>()
                .RegisterFireAndForgetFunction<Mapper, MapperCommand>()
                .RegisterFireAndForgetFunction<Reducer, ReducerCommand>()
                .RegisterFireAndForgetFunction<FinalReducer, FinalReducerCommand>()
                .RegisterFireAndForgetFunction<WorkerManager, WorkerManagerCommand>()
                .RegisterObjectStore()
                .RegisterMiscHandlers();
            
            serviceProvider = serviceCollection.BuildServiceProvider();

            return serviceProvider;
        }
    }
}