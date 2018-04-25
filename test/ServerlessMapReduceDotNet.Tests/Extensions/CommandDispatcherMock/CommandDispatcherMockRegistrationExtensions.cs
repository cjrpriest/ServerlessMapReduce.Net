using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using AzureFromTheTrenches.Commanding.Abstractions.Model;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using NSubstitute;
using NSubstitute.Core;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory;

namespace ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock
{
    public static class CommandDispatcherMockRegistrationExtensions {
        public static ICommandDispatcher Register<
            TCommand,
            TCommandHandler
        >(this ICommandDispatcher commandDispatcherMock,
            Func<TCommandHandler> commandHandlerFactory)
            where TCommand : ICommand
            where TCommandHandler : ICommandHandler<TCommand>
        {
            commandDispatcherMock
                .DispatchAsync(Arg.Any<TCommand>())
                .Returns(ci => 
                {
                    commandHandlerFactory()
                        .ExecuteAsync(ci.Arg<TCommand>())
                        .Wait();
                    return new CommandResult(false);
                });
            
            return commandDispatcherMock;
        }
        
        public static ICommandDispatcher Register<
            TCommand,
            TResult,
            TCommandHandler
        >(this ICommandDispatcher commandDispatcherMock, Func<TCommandHandler> commandHandlerFactory)
        where TCommand : ICommand<TResult>
        where TCommandHandler : ICommandHandler<TCommand, TResult>
        {
/*
      This code executes the equivalent of this...

            commandDispatcher
                .DispatchAsync(Arg.Any<TCommand>())
                .Returns(ci =>
                    new CommandResult<TResult>(
                        commandHandlerFactory()
                            .ExecuteAsync(ci.Arg<TCommand>(), default(TResult))
                            .Result,
                        false
                    )
                );
            return commandDispatcher;
      
      or this...
      
            commandDispatcherMock
                .DispatchAsync(Arg.Any<StoreObjectCommand>())
                .Returns(ci => 
                    {
                        new MemoryStoreObjectCommandHandler(timeMock, memoryObjectStoreData)
                            .ExecuteAsync(ci.Arg<StoreObjectCommand>())
                            .Wait();
                        return new CommandResult(false);
                    });
            return commandDispatcher;

      without knowing TCommand or TResult in advance
*/
//            commandDispatcherMock
//                .DispatchAsync(Arg.Any<StoreObjectCommand>())
//                .Returns(ci =>
//                {
//                    Task.FromResult(new MemoryStoreObjectCommandHandler(timeMock, memoryObjectStoreData)
//                        .ExecuteAsync(ci.Arg<StoreObjectCommand>()));
//                    return new CommandResult(false);
//                });
//            return commandDispatcherMock;

//            commandDispatcherMock
//                .DispatchAsync(Arg.Any<TCommand>())
//                .Returns(async ci =>
//                    //new Task<CommandResult<TResult>>(() =>
//                        new CommandResult<TResult>(
//                            await commandHandlerFactory()
//                                .ExecuteAsync(ci.Arg<TCommand>(), default(TResult)),
//                                //.Result,
//                            false
//                        )
//                    //)
//                );

            //Expression<Func<ICommandDispatcher, Task<int>>> e = async cdm => await return 1;

//            var dispatchAsyncExpression = RegisterCommandHandlerExpressionBuilder.BuildDispatchAsyncExpression(typeof(TResult),
//                typeof(TCommand), Expression.Parameter(typeof(ICommandDispatcher)));
//            var dispatchAsyncLambda = Expression.Lambda<Func<ICommandDispatcher, object>>(dispatchAsyncExpression).Compile();
//
//
//            var commandResultTask = commandDispatcherMock.DispatchAsync(Arg.Any<TCommand>());
////            var commandResultTask = (Task<>)dispatchAsyncLambda(commandDispatcherMock);
//                
//            commandResultTask.Returns(async ci =>
//                    new CommandResult<TResult>(
//                        await (Task<object>)commandHandlerFactory()
//                            .ExecuteAsync(ci.Arg<TCommand>(), default(TResult)),
//                        false
//                    )
//                );
            
            //new RegisterIt().DoIt<TCommand, TResult, TCommandHandler>(commandDispatcherMock, commandHandlerFactory);

//            var expression = (Action<ICommandDispatcher, object>) new RegisterCommandHandlerExpressionBuilder2().Build<TCommandHandler>().Result;
//            expression(commandDispatcherMock, commandHandlerFactory);
//            
//            return commandDispatcherMock;
            
            var registerCommandHandlerExpression = new RegisterCommandHandlerExpressionBuilder2().Build<TCommandHandler>();
            registerCommandHandlerExpression(commandDispatcherMock, commandHandlerFactory);
            
//            var registerCommandHandler = registerCommandHandlerExpression.Compile();

  //          registerCommandHandler(commandDispatcherMock, commandHandlerFactory);
            
            return commandDispatcherMock;
        }

        private static void DoIt<TCommand, TResult, TCommandHandler>(ICommandDispatcher commandDispatcher, Func<TCommandHandler> commandHandlerFactory)
            where TCommand : ICommand<TResult>
            where TCommandHandler : ICommandHandler<TCommand, TResult>
        {
            commandDispatcher
                .DispatchAsync(Arg.Any<TCommand>())
                .Returns(async ci =>
                    new CommandResult<TResult>(
                        await commandHandlerFactory()
                            .ExecuteAsync(ci.Arg<TCommand>(), default(TResult)),
                        false
                    )
                );
        }
        
        public static ConfiguredCall ReturnsCommandResult<T>(this Task<CommandResult<T>> value, T returnThis)
        {
            return value.Returns(new CommandResult<T>(returnThis, false));
        }
        
        public static ConfiguredCall ReturnsCommandResult<T>(this Task<CommandResult<T>> value, Func<T> returnThis)
        {
            return value.Returns(ci => new CommandResult<T>(returnThis(), false));
        }
       
    }
}