using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.HostingEnvironments;

namespace ServerlessMapReduceDotNet
{
    public class Program
    {
        public static void Main(params string[] args)
        {
            try
            {
                MainAsync(args).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Demystify());
            }
        }

        private static async Task MainAsync(params string[] args)
        {
            //var hostingEnvironment = new AwsLambdaHostingEnvironment();
            var hostingEnvironment = new CommandLineLocalHostingEnvironment();
            var serviceProvider = new ServiceProviderFactory().Build(hostingEnvironment);
            var config = serviceProvider.GetService<IConfig>();

            foreach (var arg in args)
                await serviceProvider.GetService<IQueueClient>().Enqueue(config.RawDataQueueName, arg);

            var commandDispatcher = serviceProvider.GetService<IFrameworkCommandDispatcher>();
#pragma warning disable 4014
            commandDispatcher.DispatchAsync(new WorkerManagerCommand());
#pragma warning restore 4014

            var sw = new Stopwatch();
            sw.Start();
            BlockUntilJobTerminates(commandDispatcher);
            sw.Stop();
            Console.WriteLine($"That took {sw.Elapsed.TotalSeconds:0.0}s");
        }

        private static async void BlockUntilJobTerminates(IFrameworkCommandDispatcher commandDispatcher)
        {
            while (!await IsTerminated(commandDispatcher))
            {
                await WaitFor(TimeSpan.FromSeconds(1), async () => !await IsTerminated(commandDispatcher));
            }
        }

        private static async Task<bool> IsTerminated(IFrameworkCommandDispatcher commandDispatcher)
        {
            var isTerinatedCommandResult = await commandDispatcher.DispatchAsync(new IsTerminatedCommand());
            return isTerinatedCommandResult.Result;
        }

        private static async Task WaitFor(TimeSpan timeSpan, Func<Task<bool>> predicate)
        {
            var returnTime = DateTime.UtcNow + timeSpan;
            while (returnTime > DateTime.UtcNow && await predicate())
                Thread.Sleep(100);
        }
    }
}