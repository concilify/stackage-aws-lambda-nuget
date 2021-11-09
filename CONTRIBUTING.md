# Contributing

## Workflows

### Prerequisites

A secret named `STACKAGE_NUGET_PUSH_TOKEN` containing a NuGet API key must have been added in order for GitHub Actions to be able to push NuGet packages

### Releasing

Tag the commit in the `main` branch that you wish to release with format `v*.*.*`. GitHub Actions will build this version and push the package to NuGet. Use format `v*.*.*-preview***` to build a pre-release NuGet package.

## Developing Locally

It's possible to build and install the Fake Runtime API locally without having to push to the NuGet repository, but you will need to uninstall the package if it's already installed.

```
dotnet tool uninstall --global Stackage.Aws.Lambda.FakeRuntime
```

From the root of the repository, build the Fake Runtime API package.

```
dotnet pack .\package\Stackage.Aws.Lambda.FakeRuntime -o .
```

From the same directory, install the package.

```
dotnet tool install --global --add-source . Stackage.Aws.Lambda.FakeRuntime
```

Alternatively, if you've built a package which has been published to NuGet, you can install it from NuGet by specifying the version number explicitly.

```
dotnet tool install --global Stackage.Aws.Lambda.FakeRuntime --version {VERSION}
```

Finally, run the tool.

```
fake-lambda-runtime
```
