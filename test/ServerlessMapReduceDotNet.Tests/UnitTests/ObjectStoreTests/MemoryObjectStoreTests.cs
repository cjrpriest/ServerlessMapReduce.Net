using AzureFromTheTrenches.Commanding.Abstractions;
using ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock;

namespace ServerlessMapReduceDotNet.Tests.UnitTests.ObjectStoreTests
{
    public class MemoryObjectStoreTests : ObjectStoreTests
    {
        protected override void RegisterObjectStore(ICommandDispatcher commandDispatcher, ITime time)
        {
            commandDispatcher.RegisterMemoryObjectStore(time);
        }
    }
}