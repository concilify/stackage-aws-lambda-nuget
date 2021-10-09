# stackage-aws-lambda-nuget

## Examples

### Building

From the root of the repository, build the lambda deployment packages.

`dotnet lambda package --project-location examples/Lambda.Basic.Example --output-package Lambda.Basic.Example.zip`

`dotnet lambda package --project-location examples/Lambda.Middleware.Example --output-package Lambda.Middleware.Example.zip`

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
