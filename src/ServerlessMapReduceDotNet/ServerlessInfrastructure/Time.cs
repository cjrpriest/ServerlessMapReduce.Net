using System;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure
{
    public class Time : ITime
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}