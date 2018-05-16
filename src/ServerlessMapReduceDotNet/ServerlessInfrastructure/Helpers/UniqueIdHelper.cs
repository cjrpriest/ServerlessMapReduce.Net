using System;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Helpers
{
    public static class UniqueIdHelper
    {
        public static string GenerateUniqueId()
        {
            return SafeEncode(Guid.NewGuid().ToByteArray());
        }

        private static string SafeEncode(byte[] toEncode) {
            var base64 = Convert.ToBase64String(toEncode);
            var safe = UrlEncodeBase64Encoded(base64);

            return safe;
        }
        
        private static string UrlEncodeBase64Encoded(string base64Encoded) {
            return base64Encoded
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", ".");
        }
    }
}