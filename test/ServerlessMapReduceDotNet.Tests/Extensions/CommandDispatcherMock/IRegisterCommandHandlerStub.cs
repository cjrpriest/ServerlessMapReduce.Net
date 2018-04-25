using System;
using AzureFromTheTrenches.Commanding.Abstractions;

namespace ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock
{
    public interface IRegisterCommandHandlerStub
    {
        void Register(ICommandDispatcher commandDispatcher, Type commandHandlerType, object commandHandler);
    }
}