# stackage-aws-lambda-nuget

## Fake Runtime

### Installation

To enable debugging lambda functions that use the custom runtime install the fake runtime package as a global tool. To use a pre-release package you will need to specify the version.

`dotnet tool install --global Stackage.Aws.Lambda.FakeRuntime [--version {VERSION}]`

### Starting up

With the fake runtime installed, run `fake-lambda-runtime` in a console to start it up.

Alternatively, if you have cloned this repository, you can build and run the fake runtime using the Powershell script `run-fake-runtime.ps1`. In which case you need not install the global tool.

## Examples

### Debugging

The two examples lambda functions are both .NET console apps so can be easily be started in debug mode in your favourite IDE, remembering to start up the fake runtime.

The `launchsettings.json` in the examples overrides the `AWS_LAMBDA_RUNTIME_API` environment variable. This follows the pattern `localhost:9001/{FUNCTION_NAME}` which allows the fake runtime to support multiple lambda functions.

When the lambda function runs it connects to the fake runtime and waits for invocations.

The lambda function can be invoked using one of the following methods:

A. cURL

Run the following in your console, where `{FUNCTION_NAME}` is the name of your lambda function. If you use a console other than Powershell, you will most likely need to alter the escaping of quotes in the JSON body.

```ps
curl -v -X POST "http://localhost:9001/2015-03-31/functions/{FUNCTION_NAME}/invocations" -H "content-type: application/json" -d '{\"foo\": \"bar\"}'
```

B. Postman

Import the `examples/Lambda Examples.postman_collection.json` file into Postman. This includes requests for both examples which can be changed to match your lambda functions.

C. AWS CLI

Run the following in your console, where `{FUNCTION_NAME}` is the name of your lambda function. Again, if you use a console other than Powershell, you will most likely need to alter the escaping of quotes in the JSON body.

```ps
aws lambda invoke --endpoint-url http://localhost:9001 --function-name {FUNCTION_NAME} --payload '{\"foo\": \"bar\"}' --cli-binary-format raw-in-base64-out response.json
```

To perform an asynchronous invocation and not wait for the response, add `--invocation-type Event` to the command.

### Deploying

From the root of the repository, build the lambda deployment packages.

`dotnet lambda package --project-location examples/Lambda.Basic.Example --output-package Lambda.Basic.Example.zip`

`dotnet lambda package --project-location examples/Lambda.Middleware.Example --output-package Lambda.Middleware.Example.zip`

These can then be deployed using the AWS Console as described in [Creating Lambda functions defined as .zip file archives](https://docs.aws.amazon.com/lambda/latest/dg/configuration-function-zip.html).

## Deployment Concerns

### Prerequisites

A secret named `NUGET_PUSH_TOKEN` containing a NuGet API key must have been added in order for GitHub Actions to be able to push NuGet packages

### Releasing

Tag the commit in the `main` branch that you wish to release with format `v*.*.*`. GitHub Actions will build this version and push the package to NuGet. Use format `v*.*.*-preview***` to build a pre-release NuGet package.

## Developing Locally

It's possible to build and install the tool locally without having to push to the NuGet repository.

You will need to uninstall the package if it's already installed.

`dotnet tool uninstall --global Stackage.Aws.Lambda.FakeRuntime`

From the root of the repository, build the fake runtime package.

`dotnet pack .\package\Stackage.Aws.Lambda.FakeRuntime -o .`

From the same directory, install the package.

`dotnet tool install --global --add-source . Stackage.Aws.Lambda.FakeRuntime`

Finally, run the tool.

`fake-lambda-runtime`
