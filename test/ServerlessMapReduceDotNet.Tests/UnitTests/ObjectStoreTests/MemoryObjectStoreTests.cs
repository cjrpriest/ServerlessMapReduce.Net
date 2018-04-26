using AzureFromTheTrenches.Commanding.Abstractions;
using NSubstitute;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock;

namespace ServerlessMapReduceDotNet.Tests.UnitTests.ObjectStoreTests
{
    public class MemoryObjectStoreTests : ObjectStoreTests2
    {
        protected override void RegisterObjectStore(ICommandDispatcher commandDispatcher, ITime time = null)
        {
            if (time == null)
                time = Substitute.For<ITime>();
            commandDispatcher.RegisterMemoryObjectStore(time);
        }
    }
}