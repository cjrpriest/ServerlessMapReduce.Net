using System.Security.Cryptography;
using System.Text;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Helpers
{
    public class HashHelper
    {
        public static string GetHashSha256(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var hasher = new SHA256Managed();
            var hash = hasher.ComputeHash(bytes);
            var hashString = new StringBuilder();
            foreach (byte x in hash)
            {
                hashString.AppendFormat("{0:x2}", x);
            }
            return hashString.ToString();
        }
    }
}