using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using AzureFromTheTrenches.Commanding.Abstractions;
using Newtonsoft.Json;

namespace ServerlessMapReduceDotNet.EntryPoints.Lambda
{
  internal class AwsLambdaCommandExecuter : ICommandExecuter, IFrameworkCommandExecuter
  {
    public async Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default (CancellationToken))
    {
      var serializerSettings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
      var commandJson = JsonConvert.SerializeObject(command, Formatting.None, serializerSettings);
      Console.WriteLine($"Sending command {commandJson} to lambda command router");
      
      var lambdaClient = new AmazonLambdaClient(RegionEndpoint.EUWest1);
      var invokeRequest = new InvokeRequest
      {
        FunctionName = "arn:aws:lambda:eu-west-1:525470265062:function:ServerlessMapReduceDotNet",
        Payload = commandJson,
        InvocationType = InvocationType.Event
      };
      await lambdaClient.InvokeAsync(invokeRequest, cancellationToken);
      return default (TResult);
    }
  }
}
