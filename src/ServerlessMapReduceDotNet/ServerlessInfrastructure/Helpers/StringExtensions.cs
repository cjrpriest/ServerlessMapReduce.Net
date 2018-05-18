namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Helpers
{
    public static class StringExtensions
    {
        public static string TopAndTail(this string inputString, int maxLength)
        {
            if (inputString.Length <= maxLength) return inputString;
            var stringBeginning = inputString.Substring(0, maxLength / 2);
            var stringEnd = inputString.Substring(inputString.Length - maxLength / 2, (maxLength / 2));
            return $"{stringBeginning}... ...{stringEnd}";
        }
    }
}