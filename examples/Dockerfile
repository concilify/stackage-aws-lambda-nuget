FROM mcr.microsoft.com/dotnet/sdk:5.0
WORKDIR /app

RUN apt-get update \
   && apt-get install -y zip \
   && dotnet tool install -g Amazon.Lambda.Tools

ENV PATH="${PATH}:/root/.dotnet/tools"

COPY ./Lambda.Basic.Example/Lambda.Basic.Example.csproj ./Lambda.Basic.Example/Lambda.Basic.Example.csproj
COPY ./Lambda.Middleware.Example/Lambda.Middleware.Example.csproj ./Lambda.Middleware.Example/Lambda.Middleware.Example.csproj

RUN dotnet restore ./Lambda.Basic.Example/Lambda.Basic.Example.csproj
RUN dotnet restore ./Lambda.Middleware.Example/Lambda.Middleware.Example.csproj

COPY . .

RUN dotnet lambda package --project-location Lambda.Basic.Example --output-package Lambda.Basic.Example.zip
RUN dotnet lambda package --project-location Lambda.Middleware.Example --output-package Lambda.Middleware.Example.zip
