using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.MapReduce.Handlers.Monitoring
{
    public class UpdateMonitoringHandler : ICommandHandler<UpdateMonitoringCommand>
    {
        private readonly IQueueClient _queueClient;
        private readonly IConfig _config;
        private readonly IWorkerRecordStoreService _workerRecordStoreService;
        private readonly ICommandDispatcher _commandDispatcher;

        public UpdateMonitoringHandler(IQueueClient queueClient, IConfig config, IWorkerRecordStoreService workerRecordStoreService, ICommandDispatcher commandDispatcher)
        {
            _queueClient = queueClient;
            _config = config;
            _workerRecordStoreService = workerRecordStoreService;
            _commandDispatcher = commandDispatcher;
        }
        
        public async Task ExecuteAsync(UpdateMonitoringCommand command)
        {
            var rawDataQueueCount = await _queueClient.MessageCount(_config.RawDataQueueName);
            var ingestedQueueCount = await _queueClient.MessageCount(_config.IngestedQueueName);
            var mappedQueueCount = await _queueClient.MessageCount(_config.MappedQueueName);
            var reducedQueueCount = await _queueClient.MessageCount(_config.ReducedQueueName);
            var finalReducedQueueCount = await _queueClient.MessageCount(_config.FinalReducedQueueName);
            var commandExecuterQueueCount = await _queueClient.MessageCount(_config.CommandQueueName);
            var remoteCommandExecuterQueueCount = await _queueClient.MessageCount(_config.RemoteCommandQueueName);

            var workerRecords = await _workerRecordStoreService.GetAllWorkerRecords();
            var ingesterWorkerRecords = workerRecords.Where(x => x.Type == "ingester" && !x.HasTerminated);
            var mapperWorkerRecords = workerRecords.Where(x => x.Type == "mapper" && !x.HasTerminated);
            var reducerWorkerRecords = workerRecords.Where(x => x.Type == "reducer" && !x.HasTerminated);
            var finalReducerWorkerRecords = workerRecords.Where(x => x.Type == "finalReducer" && !x.HasTerminated);
            var commandExecuterWorkerRecords = workerRecords.Where(x => x.Type == "commandExecuter" && !x.HasTerminated);

            var runningIngestersCount = ingesterWorkerRecords.Count();
            var runningMappersCount = mapperWorkerRecords.Count();
            var runningReducersCount = reducerWorkerRecords.Count();
            var runningFinalReducerCount = finalReducerWorkerRecords.Count();
            var runningCommandExecuterCount = commandExecuterWorkerRecords.Count();

            var htmlTemplate = LoadHtmlTemplate();
            var html = htmlTemplate
                .Replace("#rawDataQueueCount#", rawDataQueueCount.ToString())
                .Replace("#ingestedQueueCount#", ingestedQueueCount.ToString())
                .Replace("#mappedQueueCount#", mappedQueueCount.ToString())
                .Replace("#reducedQueueCount#", reducedQueueCount.ToString())
                .Replace("#runningIngestersCount#", runningIngestersCount.ToString())
                .Replace("#runningMappersCount#", runningMappersCount.ToString())
                .Replace("#runningReducersCount#", runningReducersCount.ToString())
                .Replace("#runningFinalReducersCount#", runningFinalReducerCount.ToString())
                .Replace("#finalReducedQueueCount#", finalReducedQueueCount.ToString())
                .Replace("#runningCommandExecuterCount#", runningCommandExecuterCount.ToString())
                .Replace("#commandExecuterQueueCount#", commandExecuterQueueCount.ToString())
                .Replace("#remoteCommandExecuterQueueCount#", remoteCommandExecuterQueueCount.ToString());


            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(html)))
            {
                await _commandDispatcher.DispatchAsync(new StoreObjectCommand
                {
                    Key = $"{_config.MonitoringFolder}/index.html",
                    DataStream = memoryStream
                });
            }
        }

        private string LoadHtmlTemplate()
        {
            using (var monitoringHtmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(typeof(UpdateMonitoringHandler), "Monitoring.html"))
            {
                return new StreamReader(monitoringHtmlStream).ReadToEnd();
            }

        }
    }
}