using System.Collections.Generic;
using System.Threading.Tasks;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions
{
    public interface IWorkerRecordStoreService
    {
        Task<IReadOnlyCollection<WorkerRecord>> GetAllWorkerRecords();
        Task RecordPing(string workerType, string workerId);
        Task RecordShouldStop(string workerType, string workerId);
        Task RecordHasTerminated(string workerType, string workerId);
        string GenerateUniqueId();
    }
}