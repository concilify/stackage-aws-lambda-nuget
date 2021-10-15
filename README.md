# stackage-aws-lambda-nuget

## Installation

To enable debugging lambda functions that use the custom runtime install the fake runtime package. To use a pre-release package you will need to specify the version.

`dotnet tool install --global Stackage.Aws.Lambda.FakeRuntime [--version {VERSION}]`

## Examples

### Building

From the root of the repository, build the lambda deployment packages.

`dotnet lambda package --project-location examples/Lambda.Basic.Example --output-package Lambda.Basic.Example.zip`

`dotnet lambda package --project-location examples/Lambda.Middleware.Example --output-package Lambda.Middleware.Example.zip`

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







aws lambda invoke --function-name my-function --payload '{ "key": "value" }' response.json

aws lambda invoke --endpoint-url http://localhost:9001 --function-name my-function --payload '{ "key": "value" }' response.json
aws lambda invoke --endpoint-url http://localhost:9001 --function-name my-function --invocation-type Event --payload '{ "key": "value" }' response.json

--invocation-type "RequestResponse"
--invocation-type "Event"
--invocation-type "DryRun"

