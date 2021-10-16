using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.FakeRuntime.Services;
using Stackage.Aws.Lambda.FakeRuntime.Tests.Stubs;

namespace Stackage.Aws.Lambda.FakeRuntime.Tests.ServicesTests
{
   public class FunctionsServiceTests
   {
      [Test]
      public async Task invoking_unknown_function_adds_function_and_request()
      {
         var functions = new LambdaFunction.Dictionary();
         var guidGenerator = new StubIdGenerator("arbitrary-id");

         var service = CreateService(
            functions,
            guidGenerator);

         service.Invoke("unknown-function", "arbitrary-body");

         Assert.That(functions.Count, Is.EqualTo(1));

         var function = functions["unknown-function"];
         Assert.That(function.Name, Is.EqualTo("unknown-function"));

         await AssertRequestsAreEqualAsync(function.QueuedRequests, new[] {new LambdaRequest("arbitrary-id", "arbitrary-body")});
      }

      [Test]
      public async Task invoke_known_function_without_outstanding_requests()
      {
         const string functionName = "known-function";

         var functions = new LambdaFunction.Dictionary();
         functions.TryAdd(functionName, new LambdaFunction(functionName));

         var guidGenerator = new StubIdGenerator("arbitrary-id");

         var service = CreateService(
            functions,
            guidGenerator);

         service.Invoke(functionName, "arbitrary-body");

         Assert.That(functions.Count, Is.EqualTo(1));

         var function = functions[functionName];

         await AssertRequestsAreEqualAsync(function.QueuedRequests, new[] {new LambdaRequest("arbitrary-id", "arbitrary-body")});
      }

      [Test]
      public async Task invoke_known_function_with_outstanding_request()
      {
         const string functionName = "known-function";

         var existingFunction = new LambdaFunction(functionName);
         existingFunction.QueuedRequests.Enqueue(new LambdaRequest("existing-id", "existing-body"));

         var functions = new LambdaFunction.Dictionary();
         functions.TryAdd(functionName, existingFunction);

         var guidGenerator = new StubIdGenerator("new-id");

         var service = CreateService(
            functions,
            guidGenerator);

         service.Invoke(functionName, "new-body");

         var function = functions[functionName];

         await AssertRequestsAreEqualAsync(
            function.QueuedRequests,
            new[]
            {
               new LambdaRequest("existing-id", "existing-body"),
               new LambdaRequest("new-id", "new-body")
            });
      }

      [Test]
      public void waiting_for_invocation_of_unknown_function_adds_function_and_blocks()
      {
         var functions = new LambdaFunction.Dictionary();

         var service = CreateService(functions);

         var cancellationTokenSource = new CancellationTokenSource(100);

         Assert.ThrowsAsync<OperationCanceledException>(async () =>
         {
            await service.WaitForNextInvocationAsync("unknown-function", cancellationTokenSource.Token);
         });

         Assert.That(functions.Count, Is.EqualTo(1));

         var function = functions["unknown-function"];
         Assert.That(function.Name, Is.EqualTo("unknown-function"));

         Assert.That(function.QueuedRequests.Count, Is.EqualTo(0));
      }

      [Test]
      public void waiting_for_invocation_of_known_function_without_outstanding_requests_blocks()
      {
         const string functionName = "known-function";

         var functions = new LambdaFunction.Dictionary();
         functions.TryAdd(functionName, new LambdaFunction(functionName));

         var service = CreateService(functions);

         var cancellationTokenSource = new CancellationTokenSource(100);

         Assert.ThrowsAsync<OperationCanceledException>(async () => { await service.WaitForNextInvocationAsync(functionName, cancellationTokenSource.Token); });

         Assert.That(functions.Count, Is.EqualTo(1));

         var function = functions[functionName];

         Assert.That(function.QueuedRequests.Count, Is.EqualTo(0));
      }

      [Test]
      public async Task waiting_for_invocation_of_known_function_with_outstanding_requests_returns_next_request()
      {
         const string functionName = "known-function";

         var existingFunction = new LambdaFunction(functionName);
         existingFunction.QueuedRequests.Enqueue(new LambdaRequest("first-id", "first-body"));
         existingFunction.QueuedRequests.Enqueue(new LambdaRequest("second-id", "second-body"));

         var functions = new LambdaFunction.Dictionary();
         functions.TryAdd(functionName, existingFunction);

         var service = CreateService(functions);

         var cancellationTokenSource = new CancellationTokenSource(100);

         var request = await service.WaitForNextInvocationAsync(functionName, cancellationTokenSource.Token);

         request.Should().BeEquivalentTo(new LambdaRequest("first-id", "first-body"));

         var function = functions[functionName];

         await AssertRequestsAreEqualAsync(function.QueuedRequests, new[] {new LambdaRequest("second-id", "second-body")});
      }

      [Test]
      public async Task invocation_response_is_added_to_function_responses()
      {
         var functions = new LambdaFunction.Dictionary();
         var guidGenerator = new StubIdGenerator("arbitrary-id");

         var service = CreateService(
            functions,
            guidGenerator);

         service.Invoke("unknown-function", "arbitrary-body");

         Assert.That(functions.Count, Is.EqualTo(1));

         var function = functions["unknown-function"];
         Assert.That(function.Name, Is.EqualTo("unknown-function"));

         await AssertRequestsAreEqualAsync(function.QueuedRequests, new[] {new LambdaRequest("arbitrary-id", "arbitrary-body")});
      }

      [Test]
      public void response_tests()
      {
         const string functionName = "known-function";
         const string awsRequestId = "the-request-id";

         var functions = new LambdaFunction.Dictionary();
         var existingFunction = new LambdaFunction(functionName);
         existingFunction.InFlightRequests.TryAdd(awsRequestId, new LambdaRequest(awsRequestId, "the-request-body"));

         functions.TryAdd(functionName, existingFunction);

         var service = CreateService(functions);

         service.InvocationResponse(functionName, awsRequestId, "the-response-body");

         var function = functions[functionName];

         function.CompletedRequests.Values.Should()
            .BeEquivalentTo(new[] {new LambdaCompletion(awsRequestId, "the-request-body", "the-response-body", true)});
      }

      [Test]
      public void error_tests()
      {
         const string functionName = "known-function";
         const string awsRequestId = "the-request-id";

         var functions = new LambdaFunction.Dictionary();
         var existingFunction = new LambdaFunction(functionName);
         existingFunction.InFlightRequests.TryAdd(awsRequestId, new LambdaRequest(awsRequestId, "the-request-body"));

         functions.TryAdd(functionName, existingFunction);

         var service = CreateService(functions);

         service.InvocationError(functionName, "the-request-id", "the-response-body");

         var function = functions[functionName];

         function.CompletedRequests.Values.Should()
            .BeEquivalentTo(new[] {new LambdaCompletion("the-request-id", "the-request-body", "the-response-body", false)});
      }

      private static async Task AssertRequestsAreEqualAsync(LambdaRequest.Queue requests, IList<LambdaRequest> expectedRequests)
      {
         var dequeuedRequests = new List<LambdaRequest>();

         while (requests.Count != 0)
         {
            dequeuedRequests.Add(await requests.DequeueAsync());
         }

         dequeuedRequests.Should().BeEquivalentTo(expectedRequests);
      }

      private static FunctionsService CreateService(
         LambdaFunction.Dictionary functions = null,
         IGenerateIds idGenerator = null)
      {
         functions ??= new LambdaFunction.Dictionary();
         idGenerator ??= new IdGenerator();

         return new FunctionsService(
            functions,
            idGenerator,
            A.Fake<ILogger<FunctionsService>>());
      }
   }
}
