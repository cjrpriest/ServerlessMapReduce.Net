﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using NSubstitute;

namespace ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock
{
    public class RegisterCommandHandlerExpressionBuilder2
    {
        public const string DynamicAssemblyName = "ServerlessMapReduceDotNet.Tests.DynamicHelpers";
        
        private const int CommandHandlerCommandTypeGenericArgPosition = 0;
        private const int CommandHandlerResultTypeGenericArgPosition = 1;
                
        static RegisterCommandHandlerExpressionBuilder2()
        {
            var targetAssembly = typeof(ThisAssembly).Assembly;
            var targetAssemblyExportedTypes = targetAssembly.GetTypes();
            var commandHandlerInterfaceType = typeof(ICommandHandler<>);
            var commandHandlerWithResultInterfaceType = typeof(ICommandHandler<,>);
            var commandHandlerTypes = targetAssemblyExportedTypes.Where(x => IsAssignableToGenericType(x, commandHandlerInterfaceType));
            var commandHandlerWithResultTypes = targetAssemblyExportedTypes.Where(x => IsAssignableToGenericType(x, commandHandlerWithResultInterfaceType));

            var switchOptionCodeChunks = new List<string>();
            var methodCodeChunks = new List<string>();
            
            foreach (var commandHandlerWithResultType in commandHandlerWithResultTypes)
            {
                var commandHandlerWithResultInterfaceTypeGenericArguments = commandHandlerWithResultType
                    .GetInterfaces()
                    .First(t => typeof(ICommandHandler).IsAssignableFrom(t) && t.IsGenericType)
                    .GetGenericArguments();

                var commandHandlerHasResult = commandHandlerWithResultInterfaceTypeGenericArguments.Length == 2;

                var commandType =
                    commandHandlerWithResultInterfaceTypeGenericArguments[CommandHandlerCommandTypeGenericArgPosition];
                var resultType = commandHandlerHasResult
                    ? commandHandlerWithResultInterfaceTypeGenericArguments[CommandHandlerResultTypeGenericArgPosition]
                    : null;

                // TODO need to improve this to avoid clashes -- e.g. handlebars
                var methodCode = RegisterCommandHandlerStubMethodTemplate
                    .Replace("TCommandHandler", GetFriendlyName(commandHandlerWithResultType))
                    .Replace("TCommand", GetFriendlyName(commandType))
                    .Replace("TResult", GetFriendlyName(resultType))
                    .Replace("SafeCommandHandlerName", GetSafeFriendlyName(commandHandlerWithResultType)); 
                
                methodCodeChunks.Add(methodCode);
                
                var switchOptionCode = RegisterCommandHandlerStubSwitchOptionTemplate
                    .Replace("SafeCommandHandlerName", GetSafeFriendlyName(commandHandlerWithResultType));
                
                switchOptionCodeChunks.Add(switchOptionCode);
            }

            StringBuilder switchOptionsCode = new StringBuilder();
            foreach (var switchOption in switchOptionCodeChunks)
            {
                switchOptionsCode.AppendLine(switchOption);
            }

            StringBuilder methodsCode = new StringBuilder();
            foreach (var method in methodCodeChunks)
            {
                methodsCode.AppendLine(method);
            }

            var finalCode = RegisterCommandHandlerStubClassTemplate
                .Replace("##switch_options##", switchOptionsCode.ToString())
                .Replace("##methods##", methodsCode.ToString());

            Console.WriteLine(finalCode);

            var assemblyReferences = GetAssemblyReferences();
            
            var syntaxTree = CSharpSyntaxTree.ParseText(finalCode);
            var compilation = CSharpCompilation.Create(
                DynamicAssemblyName, 
                new [] { syntaxTree },
                assemblyReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    AssemblyLoadContext.Default.LoadFromStream(ms);
                }
                else
                {
                    foreach (var resultDiagnostic in result.Diagnostics)
                    {
                        Console.WriteLine(resultDiagnostic);
                    }
                }
            }
        }

        private static PortableExecutableReference[] GetAssemblyReferences()
        {
            var metaDataReferences = new[]
            {
                MetadataReference.CreateFromFile(typeof(ICommandDispatcher).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ThisAssembly).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Substitute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location),
                    "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location),
                    "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location),
                    "System.Collections.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location),
                    "System.Threading.Tasks.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location),
                    "System.Threading.Tasks.Extensions.dll")),
            };
            return metaDataReferences;
        }

        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }
        
        public Action<ICommandDispatcher, TCommandHandler> Build<TCommandHandler>()
        {
            var dynamicAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(x => x.GetName().Name == DynamicAssemblyName);
            var registerItType = dynamicAssembly.GetType("RegisterCommandHandlerStub");
            var register = Activator.CreateInstance(registerItType) as IRegisterCommandHandlerStub;
            return (commandDispatcher, commandHandler) =>
            {
                register.Register(commandDispatcher, typeof(TCommandHandler), commandHandler);
            };
        }

        static string GetSafeFriendlyName(Type type)
        {
            var unsafeFriendlyName = GetFriendlyName(type);
            return unsafeFriendlyName
                .Replace(".", String.Empty)
                .Replace("<", String.Empty)
                .Replace(">", String.Empty);
        }

        static string GetFriendlyName(Type type)
        {
            if (type == typeof(int))
                return "int";
            else if (type == typeof(short))
                return "short";
            else if (type == typeof(byte))
                return "byte";
            else if (type == typeof(bool))
                return "bool";
            else if (type == typeof(long))
                return "long";
            else if (type == typeof(float))
                return "float";
            else if (type == typeof(double))
                return "double";
            else if (type == typeof(decimal))
                return "decimal";
            else if (type == typeof(string))
                return "string";
            else if (type.IsGenericType)
                return $"{type.Namespace}.{type.Name}".Split('`')[0] + "<" +
                       string.Join(", ", type.GetGenericArguments().Select(GetFriendlyName).ToArray()) + ">";
            else
                return $"{type.Namespace}.{type.Name}";
        }

        private const string RegisterCommandHandlerStubClassTemplate = @"
using System;
using System.Linq;
using AzureFromTheTrenches.Commanding.Abstractions;
using AzureFromTheTrenches.Commanding.Abstractions.Model;
using NSubstitute;

class RegisterCommandHandlerStub : ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock.IRegisterCommandHandlerStub
{
    public void Register(ICommandDispatcher commandDispatcher, Type commandHandlerType, object commandHandler)
    {
        switch (GetSafeFriendlyName(commandHandlerType))
        {
            ##switch_options##
            default:
                throw new NotSupportedException();

        }
    }

    ##methods##

    static string GetSafeFriendlyName(Type type)
    {
        var unsafeFriendlyName = GetFriendlyName(type);
        return unsafeFriendlyName
            .Replace(""."", String.Empty)
            .Replace(""<"", String.Empty)
            .Replace("">"", String.Empty);
    }

    static string GetFriendlyName(Type type)
    {
        if (type == typeof(int))
            return ""int"";
        else if (type == typeof(short))
            return ""short"";
        else if (type == typeof(byte))
            return ""byte"";
        else if (type == typeof(bool))
            return ""bool"";
        else if (type == typeof(long))
            return ""long"";
        else if (type == typeof(float))
            return ""float"";
        else if (type == typeof(double))
            return ""double"";
        else if (type == typeof(decimal))
            return ""decimal"";
        else if (type == typeof(string))
            return ""string"";
        else if (type.IsGenericType)
            return $""{type.Namespace}.{type.Name}"".Split('`')[0] + ""<"" +
                string.Join("", "", type.GetGenericArguments().Select(GetFriendlyName).ToArray()) + "">"";
        else
            return $""{type.Namespace}.{type.Name}"";
    }
}
";

        private const string RegisterCommandHandlerStubSwitchOptionTemplate = @"
            case ""SafeCommandHandlerName"":
                Register_SafeCommandHandlerName(commandDispatcher, commandHandler);
                break;
";

        private static string RegisterCommandHandlerStubMethodTemplate = @"
    private void Register_SafeCommandHandlerName(ICommandDispatcher commandDispatcher, object commandHandlerObj)
    {
        var commandHandler = (TCommandHandler)commandHandlerObj;

        commandDispatcher
            .DispatchAsync(Arg.Any<TCommand>())
            .Returns(async ci =>
                new CommandResult<TResult>(
                    await commandHandler
                        .ExecuteAsync(ci.Arg<TCommand>(), default(TResult)),
                    false
                )
            );
    }
";
    }
}