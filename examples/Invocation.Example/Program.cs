using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;

namespace Invocation.Example
{
   public static class Program
   {
      public static async Task Main()
      {
         var lambdaClient = new AmazonLambdaClient(RegionEndpoint.EUWest2);

         var responseTasks = Enumerable.Range(0, 20)
            .Select(_ => InvokeOneAsync(lambdaClient));

         var responses = await Task.WhenAll(responseTasks);

         Console.WriteLine(responses[0]);
      }

      private static async Task<string> InvokeOneAsync(AmazonLambdaClient lambdaClient)
      {
         var request = new InvokeRequest
         {
            FunctionName = "lambda-middleware-example",
            Payload = "{\"action\":\"delay:100\"}",
            InvocationType = InvocationType.RequestResponse
         };

         var response = await lambdaClient.InvokeAsync(request);

         using var reader = new StreamReader(response.Payload);

         return await reader.ReadToEndAsync();
      }
   }
}
