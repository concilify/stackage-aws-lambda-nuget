# Blog

## Overview

Apologies that I didn't get round to the second and third posts promised in https://acraven.medium.com/creating-an-aws-lambda-function-with-net-6-8c6b86165394 about 2 years ago. Hopefully you'll find this interesting.

.NET 8 appeared a couple of weeks ago on general release and has built on the Native AOT [***link***] that was introduced in .NET 7. I've struggled with cold start performance of Lambda functions running on .NET 6 so I was interested to see if I could leverage a project and blog post combination written 2 years ago.

The last post was published a couple of months before official AWS support for .NET 6 and this one has been published before official support for .NET 8 which is expected in January 2024. While it's possible to deploy a .NET 8 Lambda using a container, this approach uses a Custom Lambda runtime deployed on the Amazon Linux 2023 runtime.

This post not only upgrades the Stackage.Aws.Lambda NuGet package to .NET 8 but also publishes it using native AOT compilation. Native AOT is promising big performance improvements for Lambda cold starts.

## Prerequisites

You will need the .NET 8.0 SDK installed. The template used here also supports .NET 6.0 SDK but that doesn't support AOT.

If you would like to build a Lambda function .zip package that can be deployed, you will need Docker installed. And if you would like to deploy that package, you will need an AWS account.

have the previous post to hand blah blah

## Creating a Lambda function

blah

dotnet new --install Stackage.Aws.Lambda.DotNetNew.Templates

blah

dotnet new slm --name Your.Lambda.Function --framework net8.0

upload to your lambda as per the previous post

## AOT

trimming
publish aot
runtime rid

## Performance

.NET 6

.NET 8

.NET 8 w/ AOT

init time and actual time

## Deployment

Further



TODO
update README.md
update dotnet-new repo

https://nodogmablog.bryanhogan.net/2022/11/lambda-cold-starts-net-7-native-aot-vs-net-6-managed-runtime/
