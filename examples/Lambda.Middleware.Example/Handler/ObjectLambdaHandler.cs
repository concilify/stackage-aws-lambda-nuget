using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Lambda.Middleware.Example.Integrations;
using Lambda.Middleware.Example.Model;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Handler
{
   public class ObjectLambdaHandler : ILambdaHandler<APIGatewayHttpApiV2ProxyRequest<InputPoco>>
   {
      public async Task<ILambdaResult> HandleAsync(APIGatewayHttpApiV2ProxyRequest<InputPoco> request, LambdaContext context)
      {
         if (request.Body.Action == "throw")
         {
            throw new Exception("Throwing exception from ObjectLambdaHandler");
         }

         if (request.Body.Action == "delay")
         {
            await Task.Delay(1000);
         }

         return new HttpApiV2ContentResult<OutputPoco>(new OutputPoco {Action = request.Body.Action});
      }
   }
}
