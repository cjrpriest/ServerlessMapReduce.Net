using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Commands;
using ServerlessMapReduceDotNet.ObjectStore;
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
                
                using (var smallDataSetStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(typeof(Basic), "small-make-model-data-set.csv"))
                {
                    var objectStore = serviceProvider.GetService<IObjectStore>();
                    await objectStore.StoreAsync("raw/smallDataSet.csv", smallDataSetStream);
                }

                await serviceProvider.GetService<IQueueClient>().Enqueue(config.RawDataQueueName, "raw/smallDataSet.csv");

                var commandDispatcher = serviceProvider.GetService<ICommandDispatcher>();
                await commandDispatcher.DispatchAsync(new WorkerManagerCommand());

                WaitUntil(() => !IntegrationTestTerminator.ShouldRun);

                var finalObjectKey = serviceProvider.GetService<IQueueClient>().Dequeue(serviceProvider.GetService<IConfig>().FinalReducedQueueName).Result.First().Message;

                var finalReductionResult = serviceProvider.GetService<IObjectStore>().GetObjectAsString("bucket-1", finalObjectKey);
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

        private void WaitUntil(Func<bool> predicate)
        {
            while (!predicate())
                Thread.Sleep(100);
        }
    }
}