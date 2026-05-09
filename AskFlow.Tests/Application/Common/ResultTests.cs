using AskFlow.Application.Common;

namespace AskFlow.Tests.Application.Common
{
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldReturn_OkType_NoError()
        {
            var result = Result.Success();

            result.IsSuccess.Should().BeTrue();
            result.Type.Should().Be(ResultType.Ok);
            result.Error.Should().BeNull();
            result.ErrorCode.Should().BeNull();
        }

        [Theory]
        [InlineData(nameof(Result.NotFound), "CODE_NF", "not found", ResultType.NotFound)]
        [InlineData(nameof(Result.Unauthorized), "CODE_UN", "no permission", ResultType.Unauthorized)]
        [InlineData(nameof(Result.Invalid), "CODE_INV", "invalid value", ResultType.Invalid)]
        [InlineData(nameof(Result.Failure), "CODE_FAIL", "generic error", ResultType.Failure)]
        public void Static_FailureFactories_ShouldSetExpectedType(string factoryName, string code, string error, ResultType expected)
        {
            Result result = factoryName switch
            {
                nameof(Result.NotFound) => Result.NotFound(code, error),
                nameof(Result.Unauthorized) => Result.Unauthorized(code, error),
                nameof(Result.Invalid) => Result.Invalid(code, error),
                nameof(Result.Failure) => Result.Failure(code, error),
                _ => throw new ArgumentOutOfRangeException(nameof(factoryName))
            };

            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be(code);
            result.Error.Should().Be(error);
            result.Type.Should().Be(expected);
        }

        [Fact]
        public void Generic_Success_ShouldExposeValue()
        {
            var result = Result<int>.Success(42);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42);
            result.Type.Should().Be(ResultType.Ok);
            result.Error.Should().BeNull();
            result.ErrorCode.Should().BeNull();
        }

        [Theory]
        [InlineData(nameof(Result.NotFound), ResultType.NotFound)]
        [InlineData(nameof(Result.Unauthorized), ResultType.Unauthorized)]
        [InlineData(nameof(Result.Invalid), ResultType.Invalid)]
        [InlineData(nameof(Result.Failure), ResultType.Failure)]
        public void Generic_Failures_ShouldSetExpectedType_AndDefaultValue(string factoryName, ResultType expected)
        {
            Result<string> result = factoryName switch
            {
                nameof(Result.NotFound) => Result<string>.NotFound("C", "e"),
                nameof(Result.Unauthorized) => Result<string>.Unauthorized("C", "e"),
                nameof(Result.Invalid) => Result<string>.Invalid("C", "e"),
                nameof(Result.Failure) => Result<string>.Failure("C", "e"),
                _ => throw new ArgumentOutOfRangeException(nameof(factoryName))
            };

            result.IsSuccess.Should().BeFalse();
            result.Type.Should().Be(expected);
            result.Value.Should().BeNull();
            result.ErrorCode.Should().Be("C");
            result.Error.Should().Be("e");
        }
    }
}
