using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.Commands.ObjectStore;
using ServerlessMapReduceDotNet.Handlers.ObjectStore;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Abstractions;
using Shouldly;

namespace ServerlessMapReduceDotNet.Tests.IntegrationTests
{
    public class Basic
    {
        [Test]
        public async Task Test()
        {
            try
            {
                var hostingEnvironment = new IntegrationTestHostingEnvironment();
            
                var serviceProvider = new ServiceProviderFactory().Build(hostingEnvironment);
                
                var config = hostingEnvironment.ConfigFactory();

                var commandDispatcher = serviceProvider.GetService<ICommandDispatcher>();
                
                using (var smallDataSetStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(typeof(Basic), "small-make-model-data-set.csv"))
                {
                    await commandDispatcher.DispatchAsync(new StoreObjectCommand
                    {
                        Key = "raw/smallDataSet.csv",
                        DataStream = smallDataSetStream
                    });
                }

                await serviceProvider.GetService<IQueueClient>().Enqueue(config.RawDataQueueName, "raw/smallDataSet.csv");

                await commandDispatcher.DispatchAsync(new WorkerManagerCommand());

                await WaitUntil(async () => (await commandDispatcher.DispatchAsync(new IsTerminatedCommand())).Result);

                var finalObjectKey = serviceProvider.GetService<IQueueClient>().Dequeue(serviceProvider.GetService<IConfig>().FinalReducedQueueName).Result.First().Message;

                var finalReductionResult = await commandDispatcher.GetObjectAsString(finalObjectKey);
                finalReductionResult.Split(Environment.NewLine).Count(x => !string.IsNullOrEmpty(x)).ShouldBe(7);
                finalReductionResult.ShouldContain("NULL,2");
                finalReductionResult.ShouldContain("YAMAHA,1");
                finalReductionResult.ShouldContain("MERCEDES,1");
                finalReductionResult.ShouldContain("VOLKSWAGEN,1");
                finalReductionResult.ShouldContain("OTHER BRITISH,1");
                finalReductionResult.ShouldContain("BMW,2");
                finalReductionResult.ShouldContain("FORD,1");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Demystify());
                throw;
            }

        }

        private async Task WaitUntil(Func<Task<bool>> predicate)
        {
            while (!await predicate())
                Thread.Sleep(100);
        }
    }
}