using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.FakeRuntime.Services;

namespace Stackage.Aws.Lambda.FakeRuntime.Tests.ControllerTests
{
   public class RuntimeInvocationControllerTests
   {
      [Test]
      public void invocations_endpoint_without_outstanding_requests_blocks()
      {
         using var webApplicationFactory = new WebApplicationFactory<FakeRuntimeStartup>();
         using var httpClient = webApplicationFactory.CreateClient();

         // Needs sufficient time for the app to startup before being cancelled
         httpClient.Timeout = TimeSpan.FromMilliseconds(1000);

         Assert.ThrowsAsync<OperationCanceledException>(async () => { await httpClient.GetAsync("/my-function/2018-06-01/runtime/invocation/next"); });
      }

      [Test]
      public async Task invocations_endpoint_returns_outstanding_request()
      {
         var functionsService = A.Fake<IFunctionsService>();
         A.CallTo(() => functionsService.WaitForNextInvocationAsync("my-function", A<CancellationToken>._))
            .Returns(Task.FromResult(new LambdaRequest("aws-request-id", "{\"foo\":\"bar\"}")));

         using var webApplicationFactory = CreateWebApplicationFactory(functionsService);
         using var httpClient = webApplicationFactory.CreateClient();

         var response = await httpClient.GetAsync("/my-function/2018-06-01/runtime/invocation/next");

         Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

         Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("{\"foo\":\"bar\"}"));

         response.Headers.GetValues("Lambda-Runtime-Aws-Request-Id")
            .Should().BeEquivalentTo(new[] {"aws-request-id"});
         response.Headers.GetValues("Lambda-Runtime-Invoked-Function-Arn")
            .Should().BeEquivalentTo(new[] {"arn:aws:lambda:region-name:account-name:function:my-function"});

         Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
         Assert.That(response.Content.Headers.ContentType?.CharSet, Is.EqualTo("utf-8"));
      }

      [Test]
      public async Task response_endpoint_returns_okay()
      {
         var functionsService = A.Fake<IFunctionsService>();

         using var webApplicationFactory = CreateWebApplicationFactory(functionsService);
         using var httpClient = webApplicationFactory.CreateClient();

         var content = JsonContent.Create(new {bar = "foo"});

         var response = await httpClient.PostAsync("/my-function/2018-06-01/runtime/invocation/my-request-id/response", content);

         Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }

      [Test]
      public async Task response_endpoint_calls_functions_service_with_name_and_body()
      {
         var functionsService = A.Fake<IFunctionsService>();

         using var webApplicationFactory = CreateWebApplicationFactory(functionsService);
         using var httpClient = webApplicationFactory.CreateClient();

         var content = JsonContent.Create(new {bar = "foo"});

         await httpClient.PostAsync("/my-function/2018-06-01/runtime/invocation/my-request-id/response", content);

         A.CallTo(() => functionsService.InvocationResponse("my-function", "my-request-id", "{\"bar\":\"foo\"}")).MustHaveHappenedOnceExactly();
      }

      [Test]
      public async Task error_endpoint_returns_okay()
      {
         var functionsService = A.Fake<IFunctionsService>();

         using var webApplicationFactory = CreateWebApplicationFactory(functionsService);
         using var httpClient = webApplicationFactory.CreateClient();

         var content = JsonContent.Create(new {bar = "foo"});

         var response = await httpClient.PostAsync("/my-function/2018-06-01/runtime/invocation/my-request-id/error", content);

         Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }

      [Test]
      public async Task error_endpoint_calls_functions_service_with_name_and_body()
      {
         var functionsService = A.Fake<IFunctionsService>();

         using var webApplicationFactory = CreateWebApplicationFactory(functionsService);
         using var httpClient = webApplicationFactory.CreateClient();

         var content = JsonContent.Create(new {bar = "foo"});

         await httpClient.PostAsync("/my-function/2018-06-01/runtime/invocation/my-request-id/error", content);

         A.CallTo(() => functionsService.InvocationError("my-function", "my-request-id", "{\"bar\":\"foo\"}")).MustHaveHappenedOnceExactly();
      }

      private static WebApplicationFactory<FakeRuntimeStartup> CreateWebApplicationFactory(IFunctionsService functionsService)
      {
         return new WebApplicationFactory<FakeRuntimeStartup>()
            .WithWebHostBuilder(builder =>
            {
               builder.ConfigureServices(services =>
               {
                  services.AddSingleton(functionsService);
               });
            });
      }
   }
}
