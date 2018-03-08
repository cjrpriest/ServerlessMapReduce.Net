using System;

namespace ServerlessMapReduceDotNet
{
    public interface ITime
    {
        DateTime UtcNow { get; }
    }
}