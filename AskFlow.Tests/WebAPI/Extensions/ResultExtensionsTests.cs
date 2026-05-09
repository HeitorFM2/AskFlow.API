using AskFlow.Application.Common;
using AskFlow.WebAPI.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AskFlow.Tests.WebAPI.Extensions
{
    public class ResultExtensionsTests
    {
        private sealed class TestController : ControllerBase { }

        private readonly TestController _controller = new();

        [Fact]
        public void Result_Ok_NoBody_ShouldReturnNoContent()
        {
            Result.Success().ToActionResult(_controller).Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public void Result_NotFound_ShouldReturn404_WithMessage()
        {
            var action = Result.NotFound("missing").ToActionResult(_controller);
            var notFound = action.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFound.Value.Should().BeEquivalentTo(new { message = "missing" });
        }

        [Fact]
        public void Result_Unauthorized_ShouldReturn401()
        {
            var action = Result.Unauthorized("nope").ToActionResult(_controller);
            action.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public void Result_Invalid_ShouldReturn400()
        {
            var action = Result.Invalid("ruim").ToActionResult(_controller);
            action.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void Result_Failure_ShouldReturn500()
        {
            var action = Result.Failure("boom").ToActionResult(_controller);
            action.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public void GenericResult_Ok_ShouldReturn200_WithValue()
        {
            var action = Result<int>.Success(42).ToActionResult(_controller);
            action.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(42);
        }

        [Theory]
        [InlineData(ResultType.NotFound, typeof(NotFoundObjectResult))]
        [InlineData(ResultType.Unauthorized, typeof(UnauthorizedObjectResult))]
        [InlineData(ResultType.Invalid, typeof(BadRequestObjectResult))]
        public void GenericResult_Failures_ShouldMapToCorrectActionResult(ResultType type, Type expected)
        {
            Result<int> result = type switch
            {
                ResultType.NotFound => Result<int>.NotFound("e"),
                ResultType.Unauthorized => Result<int>.Unauthorized("e"),
                ResultType.Invalid => Result<int>.Invalid("e"),
                _ => throw new ArgumentOutOfRangeException()
            };

            result.ToActionResult(_controller).GetType().Should().Be(expected);
        }

        [Fact]
        public void GenericResult_Failure_ShouldReturn500()
        {
            var action = Result<int>.Failure("boom").ToActionResult(_controller);
            action.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
        }
    }
}
