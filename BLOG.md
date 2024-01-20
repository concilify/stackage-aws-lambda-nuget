# Blog

## Overview

My apologies that I didn't get round to the second and third posts promised in https://acraven.medium.com/creating-an-aws-lambda-function-with-net-6-8c6b86165394 about 2 years ago. Hopefully you'll find this interesting.

.NET 8 appeared a month ago on general release and has built on the Native AOT [***link***] that was introduced in .NET 7. Many of us have struggled with cold start performance of Lambda functions running on .NET 6 and earlier so I was interested to see if I could refresh some code written 2 years ago as part of a blog post.

The last post was published a couple of months before official AWS support for .NET 6 arrived and this one has been published before official support for .NET 8 too, which is expected in January 2024. While it's possible to deploy a .NET 8 Lambda using a container, this approach uses a Custom Lambda runtime deployed on the Amazon Linux 2023 runtime.

This post not only upgrades the Stackage.Aws.Lambda NuGet package from the original post to .NET 8, but also publishes it using native AOT compilation. Native AOT is promising big performance improvements for Lambda cold starts and I'll demonstrate that it doesn't disappoint.

## Prerequisites

You will need the .NET 8.0 SDK installed. The template used here also supports .NET 6.0 SDK but that doesn't support AOT, if you want that you'll need to target .NET 8.0.

If you would like to build a Lambda function .zip package locally that can be deployed, you will need Docker installed. And if you would like to deploy that package, you will need an AWS account.

If you have followed the original post you will have already created a Lambda function using the AWS Console. To reuse this function, you should update the Runtime to "Custom runtime on Amazon Linux 2023".

To create a new function, use "Create function" in the AWS console and select "Provide your own bootstrap on Amazon Linux 2023", accepting the other defaults. We will upload the code source shortly.

## Creating a Lambda function

Whether you've installed the template from the original post or not, you can install or update it using `dotnet new install Stackage.Aws.Lambda.DotNetNew.Templates`.

Use `dotnet new stackagelambda --name Your.Lambda.Function --framework net8.0` to create a solution containing your lambda function.

Build a `.zip` file using `build-package.ps1`; you can upload this file  to your lambda function via the AWS console or you can use the AWS CLI as follows:

```
aws lambda update-function-code \
  --function-name lambda-basic-example \
  --zip-file fileb://Lambda.Basic.Example.zip \
  --publish
```

## AOT

Having looked at publishing a solution compiled Ahead of Time, the biggest obstacle to achieving this will most likely be serialisation and deserialisation of JSON. Other reflection based code could pose a problem but I found the task of marking up potentially affected code with attributes failing painless.

trimming
publish aot
runtime rid

## Performance

.NET 6

.NET 6 w/ RTR

.NET 8

.NET 8 w/ AOT

init time and actual time

## What's stopping us

Newtonsoft
InvariantGlobalization=true

## Deployment

Further

 I wouldn't use this method for production workflows, but

https://nodogmablog.bryanhogan.net/2022/11/lambda-cold-starts-net-7-native-aot-vs-net-6-managed-runtime/

