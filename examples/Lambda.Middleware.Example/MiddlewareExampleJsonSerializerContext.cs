using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Lambda.Middleware.Example;
using Lambda.Middleware.Example.Model;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<MiddlewareExampleJsonSerializerContext>))]

namespace Lambda.Middleware.Example;

[JsonSerializable(typeof(InputPoco))]
[JsonSerializable(typeof(OutputPoco))]
public partial class MiddlewareExampleJsonSerializerContext : JsonSerializerContext
{
}
