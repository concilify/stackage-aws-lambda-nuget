FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

RUN apt-get update \
   && apt-get install -y zip clang zlib1g-dev

COPY ./Lambda.Basic.Example/Lambda.Basic.Example.csproj ./Lambda.Basic.Example/Lambda.Basic.Example.csproj
COPY ./Lambda.Middleware.Example/Lambda.Middleware.Example.csproj ./Lambda.Middleware.Example/Lambda.Middleware.Example.csproj

RUN dotnet restore ./Lambda.Basic.Example/Lambda.Basic.Example.csproj
RUN dotnet restore ./Lambda.Middleware.Example/Lambda.Middleware.Example.csproj

COPY . .

RUN dotnet publish --configuration Release --self-contained --runtime linux-x64 ./Lambda.Basic.Example/Lambda.Basic.Example.csproj
RUN dotnet publish --configuration Release --self-contained --runtime linux-x64 ./Lambda.Middleware.Example/Lambda.Middleware.Example.csproj

RUN cd /app/Lambda.Basic.Example/bin/Release/net8.0/linux-x64/publish && zip -r /app/Lambda.Basic.Example.zip .
RUN cd /app/Lambda.Middleware.Example/bin/Release/net8.0/linux-x64/publish && zip -r /app/Lambda.Middleware.Example.zip .

# aws lambda update-function-code --function-name lambda-basic-example --zip-file fileb://Lambda.Basic.Example.zip --publish
# aws lambda update-function-code --function-name lambda-basic-example --zip-file fileb://Lambda.Basic.Example.zip --publish
    --dry-run

# workflow to deploy on push to deploy branch

#dotnet publish ./Lambda.Basic.Example/Lambda.Basic.Example.csproj --output publish/basic

#aws lambda update-function-configuration --function-name lambda-basic-example --environment "Variables={COMMIT_SHA=abc,DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true}"
