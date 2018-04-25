//
//using System;
//using System.Linq;
//using AzureFromTheTrenches.Commanding.Abstractions;
//using AzureFromTheTrenches.Commanding.Abstractions.Model;
//using NSubstitute;
//
//class RegisterIt : ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock.IRegister
//{
//    public void DoIt(ICommandDispatcher commandDispatcher, Type commandHandlerType, object commandHandlerFactory)
//    {
//        switch (GetSafeFriendlyName(commandHandlerType))
//        {
//            
//            case "ServerlessMapReduceDotNetMapReduceHandlersMapperFuncHandler":
//                Register_ServerlessMapReduceDotNetMapReduceHandlersMapperFuncHandler(commandDispatcher, commandHandlerFactory);
//                break;
//
//
//            case "ServerlessMapReduceDotNetHandlersTerminateIsTerminatedCommandHandler":
//                Register_ServerlessMapReduceDotNetHandlersTerminateIsTerminatedCommandHandler(commandDispatcher, commandHandlerFactory);
//                break;
//
//
//            case "ServerlessMapReduceDotNetHandlersObjectStoreMemoryMemoryListObjectKeysCommandHandler":
//                Register_ServerlessMapReduceDotNetHandlersObjectStoreMemoryMemoryListObjectKeysCommandHandler(commandDispatcher, commandHandlerFactory);
//                break;
//
//
//            case "ServerlessMapReduceDotNetHandlersObjectStoreMemoryMemoryRetrieveObjectCommandHandler":
//                Register_ServerlessMapReduceDotNetHandlersObjectStoreMemoryMemoryRetrieveObjectCommandHandler(commandDispatcher, commandHandlerFactory);
//                break;
//
//
//            default:
//                throw new NotSupportedException();
//
//        }
//    }
//
//    
//    private void Register_ServerlessMapReduceDotNetMapReduceHandlersMapperFuncHandler(ICommandDispatcher commandDispatcher, object commandHandlerFactoryObj)
//    {
//        var commandHandlerFactory = (Func<ServerlessMapReduceDotNet.MapReduce.Handlers.MapperFuncHandler>)commandHandlerFactoryObj;
//
//        commandDispatcher
//            .DispatchAsync(Arg.Any<ServerlessMapReduceDotNet.Commands.MapperFuncCommand>())
//            .Returns(async ci =>
//                new CommandResult<ServerlessMapReduceDotNet.Model.KeyValuePairCollection>(
//                    await commandHandlerFactory()
//                        .ExecuteAsync(ci.Arg<ServerlessMapReduceDotNet.Commands.MapperFuncCommand>(), default(ServerlessMapReduceDotNet.Model.KeyValuePairCollection)),
//                    false
//                )
//            );
//    }
//
//
//    private void Register_ServerlessMapReduceDotNetHandlersTerminateIsTerminatedCommandHandler(ICommandDispatcher commandDispatcher, object commandHandlerFactoryObj)
//    {
//        var commandHandlerFactory = (Func<ServerlessMapReduceDotNet.Handlers.Terminate.IsTerminatedCommandHandler>)commandHandlerFactoryObj;
//
//        commandDispatcher
//            .DispatchAsync(Arg.Any<ServerlessMapReduceDotNet.Commands.IsTerminatedCommand>())
//            .Returns(async ci =>
//                new CommandResult<bool>(
//                    await commandHandlerFactory()
//                        .ExecuteAsync(ci.Arg<ServerlessMapReduceDotNet.Commands.IsTerminatedCommand>(), default(bool)),
//                    false
//                )
//            );
//    }
//
//
//    private void Register_ServerlessMapReduceDotNetHandlersObjectStoreMemoryMemoryListObjectKeysCommandHandler(ICommandDispatcher commandDispatcher, object commandHandlerFactoryObj)
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
//
//
//    private void Register_ServerlessMapReduceDotNetHandlersObjectStoreMemoryMemoryRetrieveObjectCommandHandler(ICommandDispatcher commandDispatcher, object commandHandlerFactoryObj)
//    {
//        var commandHandlerFactory = (Func<ServerlessMapReduceDotNet.Handlers.ObjectStore.Memory.MemoryRetrieveObjectCommandHandler>)commandHandlerFactoryObj;
//
//        commandDispatcher
//            .DispatchAsync(Arg.Any<ServerlessMapReduceDotNet.Commands.ObjectStore.RetrieveObjectCommand>())
//            .Returns(async ci =>
//                new CommandResult<System.IO.Stream>(
//                    await commandHandlerFactory()
//                        .ExecuteAsync(ci.Arg<ServerlessMapReduceDotNet.Commands.ObjectStore.RetrieveObjectCommand>(), default(System.IO.Stream)),
//                    false
//                )
//            );
//    }
//
//
//
//    static string GetSafeFriendlyName(Type type)
//    {
//        var unsafeFriendlyName = GetFriendlyName(type);
//        return unsafeFriendlyName
//            .Replace(".", String.Empty)
//            .Replace("<", String.Empty)
//            .Replace(">", String.Empty);
//    }
//
//    static string GetFriendlyName(Type type)
//    {
//        if (type == typeof(int))
//            return "int";
//        else if (type == typeof(short))
//            return "short";
//        else if (type == typeof(byte))
//            return "byte";
//        else if (type == typeof(bool))
//            return "bool";
//        else if (type == typeof(long))
//            return "long";
//        else if (type == typeof(float))
//            return "float";
//        else if (type == typeof(double))
//            return "double";
//        else if (type == typeof(decimal))
//            return "decimal";
//        else if (type == typeof(string))
//            return "string";
//        else if (type.IsGenericType)
//            return $"{type.Namespace}.{type.Name}".Split('`')[0] + "<" +
//                string.Join(", ", type.GetGenericArguments().Select(GetFriendlyName).ToArray()) + ">";
//        else
//            return $"{type.Namespace}.{type.Name}";
//    }
//}
