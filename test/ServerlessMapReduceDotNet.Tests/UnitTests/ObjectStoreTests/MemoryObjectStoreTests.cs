using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.ObjectStore;

namespace ServerlessMapReduceDotNet.Tests.UnitTests.ObjectStoreTests
{
    public class MemoryObjectStoreTests : ObjectStoreTests
    {
        protected override IObjectStore ObjectStoreFactoryImpl(ITime time)
        {
            return new MemoryObjectStore(time);
        }
    }
}