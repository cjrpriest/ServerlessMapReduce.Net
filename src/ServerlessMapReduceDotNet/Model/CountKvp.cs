namespace ServerlessMapReduceDotNet.Model
{
    public class CountKvp : KeyValuePair<string, int>
    {
        public CountKvp() { }

        public CountKvp(string key, int value)
        {
            Key = key;
            Value = value;
        }
    }
}