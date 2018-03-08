using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;

namespace ServerlessMapReduceDotNet.Handlers
{
    public class UpdateMonitoringHandler : ICommandHandler<UpdateMonitoringCommand>
    {
        private readonly IObjectStore _objectStore;
        private readonly IQueueClient _queueClient;
        private readonly IConfig _config;
        private readonly IWorkerRecordStoreService _workerRecordStoreService;

        public UpdateMonitoringHandler(IObjectStore objectStore, IQueueClient queueClient, IConfig config, IWorkerRecordStoreService workerRecordStoreService)
        {
            _objectStore = objectStore;
            _queueClient = queueClient;
            _config = config;
            _workerRecordStoreService = workerRecordStoreService;
        }
        
        public async Task ExecuteAsync(UpdateMonitoringCommand command)
        {
            var rawDataQueueCount = await _queueClient.MessageCount(_config.RawDataQueueName);
            var ingestedQueueCount = await _queueClient.MessageCount(_config.IngestedQueueName);
            var mappedQueueCount = await _queueClient.MessageCount(_config.MappedQueueName);
            var reducedQueueCount = await _queueClient.MessageCount(_config.ReducedQueueName);
            var finalReducedQueueCount = await _queueClient.MessageCount(_config.FinalReducedQueueName);

            var workerRecords = await _workerRecordStoreService.GetAllWorkerRecords();
            var ingesterWorkerRecords = workerRecords.Where(x => x.Type == "ingester" && !x.HasTerminated);
            var mapperWorkerRecords = workerRecords.Where(x => x.Type == "mapper" && !x.HasTerminated);
            var reducerWorkerRecords = workerRecords.Where(x => x.Type == "reducer" && !x.HasTerminated);
            var finalReducerWorkerRecords = workerRecords.Where(x => x.Type == "finalReducer" && !x.HasTerminated);

            var runningIngestersCount = ingesterWorkerRecords.Count();
            var runningMappersCount = mapperWorkerRecords.Count();
            var runningReducersCount = reducerWorkerRecords.Count();
            var runningFinalReducerCount = finalReducerWorkerRecords.Count();

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
                .Replace("#finalReducedQueueCount#", finalReducedQueueCount.ToString());

            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(html)))
            {
                await _objectStore.StoreAsync($"{_config.MonitoringFolder}/index.html", memoryStream);
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