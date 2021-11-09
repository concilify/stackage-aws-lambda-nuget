# Contributing

## Developing Locally

It's possible to build and install the Fake Runtime API locally without having to push to the NuGet repository, but you will need to uninstall the package if it's already installed.

```
dotnet tool uninstall --global Stackage.Aws.Lambda.FakeRuntime
```

From the root of the repository, build and install the Fake Runtime API package.

```
dotnet pack ./package/Stackage.Aws.Lambda.FakeRuntime -o .
dotnet tool install --global --add-source . Stackage.Aws.Lambda.FakeRuntime
```

Alternatively, if you've built a package which has been published to NuGet, you can install it from NuGet. You will need to specify the version number explicitly if you don't want the latest version or you want a pre-release version.

```
dotnet tool install --global Stackage.Aws.Lambda.FakeRuntime [--version {VERSION}]
```

Finally, run the tool.

```
fake-lambda-runtime
```
