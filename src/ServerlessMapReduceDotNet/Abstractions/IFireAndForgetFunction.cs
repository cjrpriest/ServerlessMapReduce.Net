using System.Threading.Tasks;

namespace ServerlessMapReduceDotNet.Abstractions
{
    public interface IFireAndForgetFunction
    {
        Task InvokeAsync();
    }
}