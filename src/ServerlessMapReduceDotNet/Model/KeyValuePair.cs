namespace ServerlessMapReduceDotNet.Model
{
    public abstract class KeyValuePair<TKey, TValue> : IKeyValuePair
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }
}