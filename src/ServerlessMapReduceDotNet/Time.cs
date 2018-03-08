using System;

namespace ServerlessMapReduceDotNet
{
    public class Time : ITime
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}