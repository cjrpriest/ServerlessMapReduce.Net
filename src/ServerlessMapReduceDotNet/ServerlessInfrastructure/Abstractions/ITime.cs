using System;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions
{
    public interface ITime
    {
        DateTime UtcNow { get; }
    }
}