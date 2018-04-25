//
//using System;
//using AzureFromTheTrenches.Commanding.Abstractions;
//using AzureFromTheTrenches.Commanding.Abstractions.Model;
//using NSubstitute;
//
//internal class RegisterIt : ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock.IRegister
//{
//    public void DoIt(ICommandDispatcher commandDispatcher, object commandHandlerFactoryObj)
//    {
//        var commandHandlerFactory = (Func<ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory.MemoryListObjectKeysCommandHandler>)commandHandlerFactoryObj;
//
//        commandDispatcher
//            .DispatchAsync(Arg.Any<ServerlessMapReduceDotNet.Commands.ObjectStore.ListObjectKeysCommand>())
//            .Returns(async ci =>
//                new CommandResult<System.Collections.Generic.IReadOnlyCollection<ServerlessMapReduceDotNet.ObjectStore.ListedObject>>(
//                    await commandHandlerFactory()
//                        .ExecuteAsync(ci.Arg<ServerlessMapReduceDotNet.Commands.ObjectStore.ListObjectKeysCommand>(), default(System.Collections.Generic.IReadOnlyCollection<ServerlessMapReduceDotNet.ObjectStore.ListedObject>)),
//                    false
//                )
//            );
//    }
//}
//return typeof(RegisterIt);
