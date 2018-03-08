using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.Functions
{
    public class WorkerManager : IWorkerManager
    {
        private readonly ICommandDispatcher _dispatcher;
        private readonly IQueueClient _queueClient;
        private readonly IConfig _config;
        private readonly IWorkerRecordStoreService _workerRecordStoreService;
        private readonly ITime _time;

        public WorkerManager(ICommandDispatcher dispatcher, IQueueClient queueClient, IConfig config, IWorkerRecordStoreService workerRecordStoreService, ITime time)
        {
            _dispatcher = dispatcher;
            _queueClient = queueClient;
            _config = config;
            _workerRecordStoreService = workerRecordStoreService;
            _time = time;
        }

        /// <summary>
        /// Invoked periodically to manage runing worker processes
        /// </summary>
        /// <returns></returns>
        public async Task InvokeAsync()
        {
            Console.WriteLine("Working manager starting...");
            
            var workerRecords = await _workerRecordStoreService.GetAllWorkerRecords();
            
            if (!await ReadyToRunFinalReducer(workerRecords))
            {
                await RegulateRunningInstances(_config.RawDataQueueName, workerRecords, "ingester", () => new IngestCommand());
                await RegulateRunningInstances(_config.IngestedQueueName, workerRecords, "mapper", () => new MapperCommand());
                await RegulateRunningInstances(new[] {_config.MappedQueueName, _config.ReducedQueueName}, workerRecords, "reducer", () => new ReducerCommand());
            }
            else
                await _dispatcher.DispatchAsync(new FinalReducerCommand());   

            await _dispatcher.DispatchAsync(new UpdateMonitoringCommand());

            Thread.Sleep(_config.SleepBetweenWorkerManagerIterationsInMs);
            
            var jobComplete = await JobComplete(workerRecords); 
            if (jobComplete)
                await _dispatcher.DispatchAsync(new TerminateProgramCommand());
            else
                await _dispatcher.DispatchAsync(new WorkerManagerCommand());
            
            await _dispatcher.DispatchAsync(new UpdateMonitoringCommand());
            
            Console.WriteLine("Worker Manager Completed...");
        }

        private async Task<bool> JobComplete(IReadOnlyCollection<WorkerRecord> workerRecords)
        {
            return ThereAreNoWorkersRunning(workerRecords) && await ThereIsJustOneItemOnTheFinalReducedQueue();
        }

        private async Task<bool> ThereIsJustOneItemOnTheFinalReducedQueue()
        {
            return await _queueClient.MessageCount(_config.RawDataQueueName) == 0
                   && await _queueClient.MessageCount(_config.IngestedQueueName) == 0
                   && await _queueClient.MessageCount(_config.MappedQueueName) == 0
                   && await _queueClient.MessageCount(_config.ReducedQueueName) == 0
                   && await _queueClient.MessageCount(_config.FinalReducedQueueName) == 1;
        }

        private async Task<bool> ReadyToRunFinalReducer(IReadOnlyCollection<WorkerRecord> workerRecords)
        {
            return ThereAreNoWorkersRunning(workerRecords) && await ThereIsJustOneItemOnTheReducedQueue();
        }

        private async Task<bool> ThereIsJustOneItemOnTheReducedQueue()
        {
            return await _queueClient.MessageCount(_config.RawDataQueueName) == 0
                   && await _queueClient.MessageCount(_config.IngestedQueueName) == 0
                   && await _queueClient.MessageCount(_config.MappedQueueName) == 0
                   && await _queueClient.MessageCount(_config.ReducedQueueName) == 1;
        }

        private bool ThereAreNoWorkersRunning(IReadOnlyCollection<WorkerRecord> workerRecords)
        {
            return !RunningWorkers(workerRecords, "ingester")
                   && !RunningWorkers(workerRecords, "mapper")
                   && !RunningWorkers(workerRecords, "reducer");
        }

        private async Task RegulateRunningInstances(string queueName, IReadOnlyCollection<WorkerRecord> workerRecords, string workerType, Func<ICommand> commandFactory)
        {
            var noOfInstancesToStart = await CalculateNumberOfInstancesToStart(queueName, workerRecords, workerType);

            await StartInstances(commandFactory, noOfInstancesToStart);
        }

        private async Task RegulateRunningInstances(string[] queueNames, IReadOnlyCollection<WorkerRecord> workerRecords, string workerType, Func<ICommand> commandFactory)
        {
            var tasks = queueNames.Select(queueName => CalculateNumberOfInstancesToStart(queueName, workerRecords, workerType));
            var listOfNoOfInstancesToStart = await Task.WhenAll(tasks);
                
            var noOfIntancesToStart = listOfNoOfInstancesToStart.Max();

            await StartInstances(commandFactory, noOfIntancesToStart);
        }

        private async Task StartInstances(Func<ICommand> commandFactory, int noOfInstancesToStart)
        {
            if (noOfInstancesToStart <= 0) return;
            
            var command = commandFactory();
            
            Console.WriteLine($"About to start {noOfInstancesToStart} instances of worker {command.GetType()}");
            
            for (var i = 0; i < noOfInstancesToStart; i++)
                await _dispatcher.DispatchAsync(command);
        }

        private async Task<int> CalculateNumberOfInstancesToStart(string queueName, IReadOnlyCollection<WorkerRecord> workerRecords, string workerType)
        {
            var queueCount = await _queueClient.MessageCount(queueName);
            if (queueCount == 0)
                return 0;
            
            var noOfRunningInstances = GetNoOfCurrentlyRunningInstances(workerRecords, workerType);
            var noOfInstancesThatShouldBeRunning = ((queueCount - 1) / _config.QueueItemsPerRunningWorker) + 1;
            var calculatedNoOfInstancesToStart = noOfInstancesThatShouldBeRunning - noOfRunningInstances;
            var noOfInstancesToStart =
                Math.Min(_config.MaxNoOfRunningWorkerInstancesPerType, calculatedNoOfInstancesToStart);

            return noOfInstancesToStart;
        }
        
        private int GetNoOfCurrentlyRunningInstances(IReadOnlyCollection<WorkerRecord> workerRecords, string workerType)
        {
            return workerRecords.Count<WorkerRecord>((Func<WorkerRecord, bool>) (x => this.IsWorkerRunningPredicate(x, workerType)));
        }

        private bool RunningWorkers(IReadOnlyCollection<WorkerRecord> records, string workerType)
        {
            return records.Any<WorkerRecord>((Func<WorkerRecord, bool>) (x => this.IsWorkerRunningPredicate(x, workerType)));
        }

        private bool IsWorkerRunningPredicate(WorkerRecord workerRecord, string workerType)
        {
            return !workerRecord.HasTerminated
                   && workerRecord.Type == workerType
                   && workerRecord.LastPingTime > (_time.UtcNow - TimeSpan.FromSeconds(30));
        }
    }
}