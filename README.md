# stackage-aws-lambda-nuget

## Custom Runtime

The `Stackage.Aws.Lambda` package contains a custom runtime for creating AWS Lambda functions using .NET Core 3.1 or .NET 6.0 that can benefit from the Dependency Injection and Middleware patterns.

You can use the `Stackage.Aws.Lambda.DotNetNew.Templates` package via `dotnet new` to create an empty AWS Lambda function which you can update with the code snippets below.

Your AWS Lambda function is a .NET console app with `Program.cs` similar to the following snippet. If you're familiar with ASP.NET Core, this won't look too unfamiliar.

```cs
   var host = LambdaHost.Create<MyRequest>(builder =>
      {
          builder.UseStartup<LambdaStartup>();
          builder.UseSerializer<CamelCaseLambdaJsonSerializer>();
          builder.UseHandler<MyLambdaHandler, MyRequest>();
      })
      .ConfigureLogging(builder =>
      {
          builder.AddJsonConsole();
      })
      .Build();

   await host.RunAsync();
```

You can configure your services and middleware using an implementation of `ILambdaStartup` as per the following snippet. The intention is that you can create middleware that you can share across all your AWS Lambda Funtions.

```cs
   public class LambdaStartup : ILambdaStartup<MyRequest>
   {
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddSingleton<ILambdaResultFactory, HttpLambdaResultFactory>();
         services.AddDeadlineCancellation();
      }

      public void ConfigurePipeline(ILambdaPipelineBuilder<MyRequest> pipelineBuilder)
      {
        pipelineBuilder.Use<RequestLoggingMiddleware<MyRequest>, MyRequest>();
        pipelineBuilder.Use<ExceptionHandlingMiddleware<MyRequest>, MyRequest>();
        pipelineBuilder.Use<DeadlineCancellationMiddleware<MyRequest>, MyRequest>();
      }
   }
```

Each AWS Lambda function must be in a separate .NET console app, but there's no reason you can't share code between them via shared packages or even house more than one per code repository.

You can use constructor injection to inject your services into the handler. A basic handler will look something like the following snippet.

```cs
   public class MyLambdaHandler : ILambdaHandler<MyRequest>
   {
      public async Task<ILambdaResult> HandleAsync(MyRequest input, ILambdaContext context)
      {
         return new StringResult("Greetings!");
      }
   }
```

## Fake Runtime API

### Installation

To enable debugging AWS Lambda functions that use the custom runtime, install the `Stackage.Aws.Lambda.FakeRuntime` package as a global tool using the `dotnet tool install --global` command.

```
dotnet tool install --global Stackage.Aws.Lambda.FakeRuntime
```

To update to the latest version of the `Stackage.Aws.Lambda.FakeRuntime` package use the `dotnet tool update --global` command.

```
dotnet tool update --global Stackage.Aws.Lambda.FakeRuntime
```

### Usage

With the Fake Runtime API installed, run `fake-lambda-runtime` in a console to start it up.

Alternatively, if you have cloned this repository, you can build and run the Fake Runtime API using the Powershell script `run-fake-runtime.ps1`. In which case you need not install the global tool.

## Examples

### Debugging

The two example AWS Lambda functions are both .NET console apps so can be easily be started in debug mode in your favourite IDE, remembering to start up the Fake Runtime API.

The `launchsettings.json` in the examples overrides the `AWS_LAMBDA_RUNTIME_API` environment variable. This follows the pattern `localhost:9001/{FUNCTION_NAME}` which allows the Fake Runtime API to support multiple functions.

When the AWS Lambda function bootstraps it connects to the Fake Runtime API and waits for invocations. The function can be invoked using one of the following methods.

A. cURL

Run the following in your console, where `{FUNCTION_NAME}` is the name of your function. If you use a console other than Powershell, you will most likely need to alter the escaping of quotes in the JSON body.

```ps
curl -v -X POST "http://localhost:9001/2015-03-31/functions/{FUNCTION_NAME}/invocations" -H "content-type: application/json" -d '{"foo": "bar"}'
```

B. Postman

Import the `examples/Lambda Examples.postman_collection.json` file into Postman. This includes requests for both examples which can be changed to match your functions.

C. AWS CLI

Run the following in your console, where `{FUNCTION_NAME}` is the name of your function. Again, if you use a console other than Powershell, you will most likely need to alter the escaping of quotes in the JSON body.

```ps
aws lambda invoke --endpoint-url http://localhost:9001 --function-name {FUNCTION_NAME} --payload '{"foo": "bar"}' --cli-binary-format raw-in-base64-out response.json
```

To perform an asynchronous invocation and not wait for the response, add `--invocation-type Event` to the command.

### Deploying

From the root of the repository, build the example AWS Lambda deployment packages using the `build-examples` script.

`.\build-examples.ps1`

This uses Docker to build the AWS Lambda deployment packages `Lambda.Basic.Example.zip` and `Lambda.Middleware.Example.zip` which are optimised for Linux.

These can then be deployed using the AWS Console as described in [Creating Lambda functions defined as .zip file archives](https://docs.aws.amazon.com/lambda/latest/dg/configuration-function-zip.html).

## Contributing

If you would like to contribute, please read through the [CONTRIBUTING.md](./CONTRIBUTING.md) document.
