# Stackage.LambdaPackage

## Debugging

### Install the Fake Runtime API

Install the Fake Runtime API package as a global tool if you haven't done so already.

`dotnet tool install --global Stackage.Aws.Lambda.FakeRuntime [--version {VERSION}]`

### Start the Fake Runtime API

`fake-lambda-runtime`

### Start the Lambda function runtime

The Lambda functions is bootstrapped as a .NET console app so can be easily be started in debug mode in your favourite IDE. Use the `Stackage.LambdaPackage` profile in `launchSettings.json` or otherwise ensure the `AWS_LAMBDA_RUNTIME_API` is set to `localhost:9001/Stackage.LambdaPackage`.

### Invoke the Lambda function

The Lambda function can be invoked using one of the following methods:

A. cURL

Run the following in your console. If you use a console other than Powershell, you will most likely need to alter the escaping of quotes in the JSON body.

```ps
curl -v -X POST "http://localhost:9001/2015-03-31/functions/Stackage.LambdaPackage/invocations" -H "content-type: application/json" -d '{\"name\": \"FOO\"}'
```

B. Postman

Import the `Stackage.LambdaPackage.postman_collection.json` file into Postman, select the `Invoke Lambda` request of the `Stackage.LambdaPackage` collection and click `Send`.

C. AWS CLI

Run the following in your console. Again, if you use a console other than Powershell, you will most likely need to alter the escaping of quotes in the JSON body.

```ps
aws lambda invoke --endpoint-url http://localhost:9001 --function-name Stackage.LambdaPackage --payload '{\"name\": \"FOO\"}' --cli-binary-format raw-in-base64-out response.json
```

To perform an asynchronous invocation and not wait for the response, add `--invocation-type Event` to the command.
