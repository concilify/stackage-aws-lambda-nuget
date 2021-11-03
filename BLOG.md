# blog-lambda-runtime

## Background

I've inherited a code-base containing multiple lambdas that use a custom framework. The custom framework has been written since the AWS lambda runtime (that actually bootstraps and invokes the handlers) doesn't support Dependency Injection (DI) out of the box.

The custom framework provides an interface of which all the lambdas implement, which is resolved from a DI container (when the lambda is invoked) allowing for each abstracted lambda handler to consume constructor injected dependencies. It also includes the actual lambda handler (eg. `{returnType} {handlerName}({inputType} input, ILambdaContext context)`) and proxies this to the resolved concrete lambda handler.

The first time a handler is invoked is known as a cold-start where the assemblies are loaded up and any initialisation code is run. Subsequent invocations are known as warm-starts and result in much lower latency as the code has already been initialised. The AWS runtime keeps the handler warm for, I believe, an undefined period of time after it was last invoked. More information can be found online by searching for AWS Lambda cold-starts.

The custom framework provides some hooks for DI customisation that are executed the first time a handler is invoked during a cold-start and the resulting `IServiceProvider` is stored in a field for the remaining life of the instance for subsequent warm-starts. From this point though, it isn't quite as straight forward as resolving the abstract lambda handler from the `IServiceProvider` and invoking it; there is what I can best describe as transaction script between the two.

The transaction script is performing what I would describe as inline middleware; performing tasks such as payload and correlation ID parsing, setting logging scope properties, request and response logging and cancellation token handling of remaining time. The result being it is incredibly difficult to test drive changes to this behaviour as it's all tightly coupled, it's also excessively complicated to be able to debug any of the lambda handlers in an IDE.

Those of you familiar with ASP.NET will no doubt have come across the concept of middleware. This is a pattern whereby a pipeline can be created by chaining together one or more middleware components. Each is able to perform some logic before and/or after the next middleware component, before ultimately invoking the desired logic at the end of the pipeline.

I wanted to see if I could improve this custom framework by applying the middleware pattern, removing the clumbsy caching of the DI container by the actual lambda handler and enabling easy debugging. What I ended up with was a completely new [AWS Lambda runtime](https://docs.aws.amazon.com/lambda/latest/dg/runtimes-custom.html) and a tool that emulates locally the [AWS Lambda runtime API](https://docs.aws.amazon.com/lambda/latest/dg/runtimes-api.html) which handles communication between the custom runtime and the Lambda execution environment.

## TL;DR;

The tools I've created can be found on NuGet, the runtime is [Stackage.Aws.Lambda](https://www.nuget.org/packages/Stackage.Aws.Lambda) and the fake runtime API is [Stackage.Aws.Lambda.FakeRuntime](https://www.nuget.org/packages/Stackage.Aws.Lambda.FakeRuntime).

These can be found in the https://github.com/concilify/stackage-aws-lambda-nuget repository on GitHub which contain a couple of examples on how to use them to create and debug lambda functions. More information can be found in the `README.md` file of the repository.

## Creating a lambda function

Create a new .NET console app in your IDE or using `dotnet new console`. Any language capable of building a console app can be used to create a lambda function, which itself is called from a known `bootstrap` script which will be part of the .zip file we will package our lambda function in for uploading to AWS Lambda.

Having created a console app named `Lambda.Basic.Example`, I've added a `bootstrap` file:

```sh
#!/bin/sh
/var/task/Lambda.Basic.Example
```

After adding the `Stackage.Aws.Lambda` and `Amazon.Lambda.Serialization.SystemTextJson` NuGet packages to your console app project, replace the contents of `Program.cs` with:

```cs
using System.Threading.Tasks;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda;
using Stackage.Aws.Lambda.Extensions;

namespace Lambda.Basic.Example
{
    public static class Program
    {
        public static async Task Main()
        {
            var host = LambdaHost.Create<BasicRequest>(builder =>
                {
                    builder.UseSerializer<CamelCaseLambdaJsonSerializer>();
                    builder.UseHandler<BasicLambdaHandler, BasicRequest>();
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddJsonConsole();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
```

The NuGet package uses the .NET Generic Host to bootstrap DI container creation and startup of the task listening to the Lambda runtime API. Here we have specified the serializer that will be use and also the handler, which we will define shortly. You will see later how we can also specify a startup class allowing us to configure the DI container.

Each console app is intended to host only a single lambda function, above we have specified we are using `BasicLambdaHandler` which accepts a payload of type `BasicRequest`.

```cs
   public class BasicLambdaHandler : ILambdaHandler<BasicRequest>
   {
      public Task<ILambdaResult> HandleAsync(BasicRequest request, LambdaContext context)
      {
         ILambdaResult result = new StringResult($"Greetings {request.Name}!");

         return Task.FromResult(result);
      }
   }

   public class BasicRequest
   {
      public string Name { get; set; }
   }
```

We can almost debug this locally, but we need to hook it to the fake runtime API. Install the fake runtime API using `dotnet tool install --global Stackage.Aws.Lambda.FakeRuntime` and add a `Properties/launchSettings.json` file:

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "Lambda.Basic.Example": {
      "commandName": "Project",
      "environmentVariables": {
        "DOTNET_ENVIRONMENT": "Development",
        "AWS_LAMBDA_RUNTIME_API": "localhost:9001/basic"
      }
    }
  }
}
```

The `AWS_LAMBDA_RUNTIME_API` variable is the important bit here; the `/basic` suffix allows the fake runtime API to support multiple lambdas.

In your console use `fake-lambda-runtime` to start the runtime and start your lambda console app in your IDE. Once startup has completed, you can invoke the lambda function; this can be done using cURL for example (if you're using something other than Powershell, you may need to escape the quotes differently):

```ps
curl -v -X POST "http://localhost:9001/2015-03-31/functions/basic/invocations" -H "content-type: application/json" -d '{\"name\": \"Andrew\"}'
```

Or using the AWS CLI:

```ps
aws lambda invoke --endpoint-url http://localhost:9001 --function-name basic --payload '{\"name\": \"bar\"}' --cli-binary-format raw-in-base64-out response.json
```


* Build .zip for deploy
* Request and response body logging
* Cancellation based on remaining time


The custom framework includes the actual lambda handler `{returnType} {handlerName}({inputType} input, ILambdaContext context)` and proxies this to an abstraction of a lambda handler resolvable through DI.


The instance isn't the handler itself, rather an instance which sub-classes a base class extending it

Correlation Ids/Request Ids
Logging BeginScope
Request and response body logging
Cancellation based on remaining time
Finally calling the handler

All this is in a single method of a base class which is sub-classed to allow the
from which the lambda functions extend adding their own behaviour.


This sounds like what I'm looking for. I've inherited a code-base containing multiple lambdas that using a custom framework; the framework provides some hooks for DI customisation that are executed the first time a handler is invoked during a cold start and the IServiceProvider is stored in a field for the remaining life of the instance for warm starts.

The concept of initialising the generic host when the lambda cold starts makes perfect sense to me, and would allow me to separate the initialisation and implementation concerns that are clumsily bundled together in our code-base.

https://github.com/aws/aws-lambda-dotnet/issues/892
https://github.com/aws/aws-lambda-dotnet/issues/882
https://github.com/aws/aws-lambda-dotnet/issues/763
https://github.com/aws/aws-lambda-dotnet/issues/709

Have tried lambda local

Opinionated debugger
