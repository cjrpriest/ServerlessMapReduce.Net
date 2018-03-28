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
            BlockUntilJobTerminates();
            sw.Stop();
            Console.WriteLine($"That took {sw.Elapsed.TotalSeconds:0.0}s");
        }

        private static void BlockUntilJobTerminates()
        {
            while (CommandLineTerminator.ShouldRun)
            {
                WaitFor(TimeSpan.FromSeconds(1), () => CommandLineTerminator.ShouldRun);
            }
        }

        private static void WaitFor(TimeSpan timeSpan, Func<bool> shouldRun)
        {
            var returnTime = DateTime.UtcNow + timeSpan;
            while (returnTime > DateTime.UtcNow && shouldRun())
                Thread.Sleep(100);
        }
    }
}