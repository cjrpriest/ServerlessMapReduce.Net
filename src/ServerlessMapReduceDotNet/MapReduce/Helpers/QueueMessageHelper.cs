using System.Text.RegularExpressions;

namespace ServerlessMapReduceDotNet.MapReduce.Helpers
{
    public static class QueueMessageHelper
    {
        private static readonly Regex KeyRegex = new Regex(@".*/(?<objectName>.*?)$", RegexOptions.Compiled);

        public static string ObjectName(this string queueMessage)
        {
            var objectName = KeyRegex.Match(queueMessage).Groups["objectName"].Value;
            return objectName;
        }
    }
}