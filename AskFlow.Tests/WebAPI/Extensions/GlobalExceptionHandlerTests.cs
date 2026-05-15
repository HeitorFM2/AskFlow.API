using System.Text.Json;
using AskFlow.Application.Common;
using AskFlow.WebAPI.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.WebAPI.Extensions
{
    public class GlobalExceptionHandlerTests
    {
        private readonly GlobalExceptionHandler _sut = new(Substitute.For<ILogger<GlobalExceptionHandler>>());

        private static async Task<(int Status, JsonElement Body)> Handle(GlobalExceptionHandler sut, Exception exception)
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await sut.TryHandleAsync(context, exception, CancellationToken.None);

            context.Response.Body.Position = 0;
            using var reader = new StreamReader(context.Response.Body);
            using var doc = JsonDocument.Parse(await reader.ReadToEndAsync());
            return (context.Response.StatusCode, doc.RootElement.Clone());
        }

        [Fact]
        public async Task TryHandleAsync_ValidationException_ShouldReturn400_WithValidationPayload()
        {
            var failure = new ValidationFailure("Page", "Page must be greater than or equal to 1.")
            {
                ErrorCode = ErrorCodes.PaginationPageInvalid
            };

            var (status, body) = await Handle(_sut, new ValidationException(new[] { failure }));

            status.Should().Be(StatusCodes.Status400BadRequest);
            body.GetProperty("code").GetString().Should().Be(ErrorCodes.ValidationError);

            var error = body.GetProperty("errors").EnumerateArray().Single();
            error.GetProperty("code").GetString().Should().Be(ErrorCodes.PaginationPageInvalid);
            error.GetProperty("field").GetString().Should().Be("Page");
            error.GetProperty("message").GetString().Should().Be("Page must be greater than or equal to 1.");
        }

        [Fact]
        public async Task TryHandleAsync_UnhandledException_ShouldReturn500_WithTraceId()
        {
            var (status, body) = await Handle(_sut, new InvalidOperationException("boom"));

            status.Should().Be(StatusCodes.Status500InternalServerError);
            body.GetProperty("code").GetString().Should().Be(ErrorCodes.InternalServerError);
            body.TryGetProperty("traceId", out _).Should().BeTrue();
        }

        [Fact]
        public async Task TryHandleAsync_AnyException_ShouldReturnTrue()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var handled = await _sut.TryHandleAsync(context, new Exception(), CancellationToken.None);

            handled.Should().BeTrue();
        }
    }
}
