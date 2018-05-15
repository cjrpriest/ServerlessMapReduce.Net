using System.Threading.Tasks;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions
{
    public interface IFireAndForgetFunction
    {
        Task InvokeAsync();
    }
}