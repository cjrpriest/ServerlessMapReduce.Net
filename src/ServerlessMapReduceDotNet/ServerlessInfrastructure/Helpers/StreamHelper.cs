using System.IO;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Helpers
{
    public static class StreamHelper
    {
        public static Stream NewEmptyStream()
        {
            return new MemoryStream(new byte[] {});
        }
    }
}