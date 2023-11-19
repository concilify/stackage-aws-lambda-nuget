using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions;

public interface ILambdaResult
{
   Task ExecuteResultAsync(ILambdaContext context, IServiceProvider requestServices);
}
