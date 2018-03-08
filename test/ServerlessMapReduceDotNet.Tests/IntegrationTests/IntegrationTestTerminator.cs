using ServerlessMapReduceDotNet.Abstractions;

namespace ServerlessMapReduceDotNet.Tests.IntegrationTests
{
    class IntegrationTestTerminator : ITerminator
    {
        public static bool ShouldRun = true;
        
        public void Terminate()
        {
            ShouldRun = false;
        }
    }
}