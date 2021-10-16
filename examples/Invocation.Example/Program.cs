using System;
using System.IO;
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

         var request = new InvokeRequest
         {
            FunctionName = "lambda-middleware-example",
            Payload = "{\"action\":\"throw\"}",
            InvocationType = InvocationType.RequestResponse
         };

         var response = await lambdaClient.InvokeAsync(request);

         using var reader = new StreamReader(response.Payload);

         var payload = await reader.ReadToEndAsync();

         Console.WriteLine(payload);
      }
   }
}
