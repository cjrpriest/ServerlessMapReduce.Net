using ServerlessMapReduceDotNet.Abstractions;

namespace ServerlessMapReduceDotNet.HostingEnvironments
{
    class CommandLineTerminator : ITerminator
    {
        public static bool ShouldRun = true;

        public void Terminate()
        {
            ShouldRun = false;
        }
    }
}