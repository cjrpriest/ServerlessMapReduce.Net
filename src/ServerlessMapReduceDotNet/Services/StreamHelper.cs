using System.IO;

namespace ServerlessMapReduceDotNet.Services
{
    public static class StreamHelper
    {
        public static Stream NewEmptyStream()
        {
            return new MemoryStream(new byte[] {});
        }
    }
}