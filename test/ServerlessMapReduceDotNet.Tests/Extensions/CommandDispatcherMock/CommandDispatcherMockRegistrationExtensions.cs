﻿using System;
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
            TCommandHandler
        >(this ICommandDispatcher commandDispatcherMock, TCommandHandler commandHandler)
        where TCommandHandler : ICommandHandler
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
            var registerCommandHandlerExpression = new RegisterCommandHandlerExpressionBuilder2().Build<TCommandHandler>();
            registerCommandHandlerExpression(commandDispatcherMock, commandHandler);
            
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