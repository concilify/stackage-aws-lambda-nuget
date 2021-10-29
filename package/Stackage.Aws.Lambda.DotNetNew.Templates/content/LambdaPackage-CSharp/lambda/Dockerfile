FROM mcr.microsoft.com/dotnet/sdk:5.0
WORKDIR /lambda

RUN apt-get update \
   && apt-get install -y zip \
   && dotnet tool install -g Amazon.Lambda.Tools

ENV PATH="${PATH}:/root/.dotnet/tools"

COPY ./Stackage.LambdaPackage/Stackage.LambdaPackage.csproj ./Stackage.LambdaPackage/Stackage.LambdaPackage.csproj

RUN dotnet restore ./Stackage.LambdaPackage/Stackage.LambdaPackage.csproj

COPY . .

RUN dotnet lambda package --project-location Stackage.LambdaPackage --output-package Stackage.LambdaPackage.zip
