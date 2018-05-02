using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NSubstitute;

namespace ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock
{
    public class RegisterCommandHandlerStubActionBuilder
    {
        private const string DynamicAssemblyName = "ServerlessMapReduceDotNet.Tests.DynamicHelpers";
        
        private const int CommandHandlerCommandTypeGenericArgPosition = 0;
        private const int CommandHandlerResultTypeGenericArgPosition = 1;
                
        static RegisterCommandHandlerStubActionBuilder()
        {
            var sourceCode = GenerateSourceCode();

            var sourceCodeHash = GetHash(sourceCode);
            var hashOfCachedCode = GetCurrentHash();

            if (sourceCodeHash == hashOfCachedCode) return;
            
            WriteHash(sourceCodeHash);
            var sw = new Stopwatch();
            sw.Start();
            var assemblyIl = GenerateAssembly(sourceCode);
            sw.Stop();
            Console.WriteLine($"Generating assembly took {sw.ElapsedMilliseconds}ms");
            File.WriteAllBytes(GetDynamicAssemblyFileName(), assemblyIl);
        }

        private static string GetHashFileName()
        {
            return GetLocationNextToExecutingAssembly($"{DynamicAssemblyName}.dll.srcHash");

        }
        
        private static string GetDynamicAssemblyFileName()
        {
            return GetLocationNextToExecutingAssembly($"{DynamicAssemblyName}.dll");
        }

        private static string GetLocationNextToExecutingAssembly(string fileName)
        {
            var currentAssemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            return Path.Combine(currentAssemblyDirectory, fileName);
        }

        private static string GetCurrentHash()
        {
            try
            {
                return File.ReadAllText(GetHashFileName());
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        private static void WriteHash(string hash)
        {
            try
            {
                File.WriteAllText(GetHashFileName(), hash);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        
        private static string GetHash(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var hashstring = new SHA256Managed();
            var hash = hashstring.ComputeHash(bytes);
            var hashString = new StringBuilder();
            foreach (byte x in hash)
            {
                hashString.Append(String.Format("{0:x2}", x));
            }
            return hashString.ToString();
        }

        private static byte[] GenerateAssembly(string sourceCode)
        {
            var assemblyReferences = GetAssemblyReferences();

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var compilation = CSharpCompilation.Create(
                DynamicAssemblyName,
                new[] {syntaxTree},
                assemblyReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (result.Success)
                {
                    return ms.ToArray();
                }

                foreach (var resultDiagnostic in result.Diagnostics)
                {
                    Console.WriteLine(resultDiagnostic);
                }
                throw new ApplicationException("Unable to compile generated command handler stub code");
            }
        }

        private static string GenerateSourceCode()
        {
            var targetAssembly = typeof(ThisAssembly).Assembly;
            var targetAssemblyExportedTypes = targetAssembly.GetTypes();
            var commandHandlerInterfaceType = typeof(ICommandHandler<>);
            var commandHandlerWithResultInterfaceType = typeof(ICommandHandler<,>);
            
            var commandHandlerTypes =
                targetAssemblyExportedTypes
                    .Where(x => 
                        IsAssignableToGenericType(x, commandHandlerInterfaceType)
                        && !x.IsGenericType);
            
            var commandHandlerWithResultTypes =
                targetAssemblyExportedTypes
                    .Where(x => 
                        IsAssignableToGenericType(x, commandHandlerWithResultInterfaceType)
                        && !x.IsGenericType);

            var switchOptionCodeChunks = new List<string>();
            var methodCodeChunks = new List<string>();

            foreach (var commandHandlerType in commandHandlerTypes)
            {
                var commandHandlerInterfaceTypeGenericArguments = commandHandlerType
                    .GetInterfaces()
                    .First(t => typeof(ICommandHandler).IsAssignableFrom(t) && t.IsGenericType)
                    .GetGenericArguments();
                
                var commandType =
                    commandHandlerInterfaceTypeGenericArguments[CommandHandlerCommandTypeGenericArgPosition];
                
                var methodCode = RegisterCommandHandlerStubMethodTemplate
                    .Replace("TCommandHandler", GetFriendlyName(commandHandlerType))
                    .Replace("TCommand", GetFriendlyName(commandType))
                    .Replace("SafeCommandHandlerName", GetSafeFriendlyName(commandHandlerType));
                
                methodCodeChunks.Add(methodCode);

                var switchOptionCode = RegisterCommandHandlerStubSwitchOptionTemplate
                    .Replace("SafeCommandHandlerName", GetSafeFriendlyName(commandHandlerType));

                switchOptionCodeChunks.Add(switchOptionCode);
            }
            
            foreach (var commandHandlerWithResultType in commandHandlerWithResultTypes)
            {
                var commandHandlerWithResultInterfaceTypeGenericArguments = commandHandlerWithResultType
                    .GetInterfaces()
                    .First(t => typeof(ICommandHandler).IsAssignableFrom(t) && t.IsGenericType)
                    .GetGenericArguments();

                var commandType =
                    commandHandlerWithResultInterfaceTypeGenericArguments[CommandHandlerCommandTypeGenericArgPosition];
                var resultType = commandHandlerWithResultInterfaceTypeGenericArguments[CommandHandlerResultTypeGenericArgPosition];

                // TODO need to improve this to avoid clashes -- e.g. handlebars
                var methodCode = RegisterCommandHandlerWithResultStubMethodTemplate
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

            var code = RegisterCommandHandlerStubClassTemplate
                .Replace("##switch_options##", switchOptionsCode.ToString())
                .Replace("##methods##", methodsCode.ToString());
            return code;
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
            var dynamicAssembly = Assembly.LoadFile(GetDynamicAssemblyFileName());
            var registerCommandHandlerStubType = dynamicAssembly.GetType("RegisterCommandHandlerStub");
            
            if (!(Activator.CreateInstance(registerCommandHandlerStubType) is IRegisterCommandHandlerStub registerCommandHandlerStub))
                throw new ApplicationException("Instance of RegisterCommandHandlerStub cannot be cast to IRegisterCommandHandlerStub");
            
            return (commandDispatcherMock, commandHandler) =>
            {
                registerCommandHandlerStub.Register(commandDispatcherMock, typeof(TCommandHandler), commandHandler);
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
            if (type == typeof(short))
                return "short";
            if (type == typeof(byte))
                return "byte";
            if (type == typeof(bool))
                return "bool";
            if (type == typeof(long))
                return "long";
            if (type == typeof(float))
                return "float";
            if (type == typeof(double))
                return "double";
            if (type == typeof(decimal))
                return "decimal";
            if (type == typeof(string))
                return "string";
            if (type.IsGenericType)
                return $"{type.Namespace}.{type.Name}".Split('`')[0] + "<" +
                       string.Join(", ", type.GetGenericArguments().Select(GetFriendlyName).ToArray()) + ">";
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

        private const string RegisterCommandHandlerStubSwitchOptionTemplate = 
@"            case ""SafeCommandHandlerName"":
                Register_SafeCommandHandlerName(commandDispatcher, commandHandler);
                break;";

        private static string RegisterCommandHandlerWithResultStubMethodTemplate =
@"    private void Register_SafeCommandHandlerName(ICommandDispatcher commandDispatcher, object commandHandlerObj)
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
    }";
        
        private static string RegisterCommandHandlerStubMethodTemplate =
@"    private void Register_SafeCommandHandlerName(ICommandDispatcher commandDispatcher, object commandHandlerObj)
    {
        var commandHandler = (TCommandHandler)commandHandlerObj;

        commandDispatcher
            .DispatchAsync(Arg.Any<TCommand>())
            .Returns(async ci => 
            {
                await commandHandler
                    .ExecuteAsync(ci.Arg<TCommand>());
                return new CommandResult(false);
            });
    }";
    }
}