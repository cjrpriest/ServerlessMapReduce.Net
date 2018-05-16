using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using AzureFromTheTrenches.Commanding.Abstractions;
using AzureFromTheTrenches.Commanding.Abstractions.Model;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.HostingEnvironments;

namespace ServerlessMapReduceDotNet.EntryPoints.Lambda
{
    public class LambdaCommandRouter
    {
        private static readonly Lazy<IServiceProvider> ServiceProvider = new Lazy<IServiceProvider>(GetServiceProvider);

        [LambdaSerializer(typeof (CommandSerialiser))]
        public async Task Route(NoResultCommandWrapper command, ILambdaContext context)
        {
            try
            {
                var executer = ServiceProvider.Value.GetService<IDirectCommandExecuter>();
                Console.WriteLine(command.Command.GetType());
                await executer.ExecuteAsync(command);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Demystify());
            }
        }

        private static IServiceProvider GetServiceProvider()
        {
            return new ServiceProviderFactory().Build(new AwsLambdaHostingEnvironment());
        }
    }
}