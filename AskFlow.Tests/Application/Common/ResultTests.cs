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
        }

        [Theory]
        [InlineData(nameof(Result.NotFound), "não achou", ResultType.NotFound)]
        [InlineData(nameof(Result.Unauthorized), "sem permissão", ResultType.Unauthorized)]
        [InlineData(nameof(Result.Invalid), "valor inválido", ResultType.Invalid)]
        [InlineData(nameof(Result.Failure), "erro genérico", ResultType.Failure)]
        public void Static_FailureFactories_ShouldSetExpectedType(string factoryName, string error, ResultType expected)
        {
            Result result = factoryName switch
            {
                nameof(Result.NotFound) => Result.NotFound(error),
                nameof(Result.Unauthorized) => Result.Unauthorized(error),
                nameof(Result.Invalid) => Result.Invalid(error),
                nameof(Result.Failure) => Result.Failure(error),
                _ => throw new ArgumentOutOfRangeException(nameof(factoryName))
            };

            result.IsSuccess.Should().BeFalse();
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
                nameof(Result.NotFound) => Result<string>.NotFound("e"),
                nameof(Result.Unauthorized) => Result<string>.Unauthorized("e"),
                nameof(Result.Invalid) => Result<string>.Invalid("e"),
                nameof(Result.Failure) => Result<string>.Failure("e"),
                _ => throw new ArgumentOutOfRangeException(nameof(factoryName))
            };

            result.IsSuccess.Should().BeFalse();
            result.Type.Should().Be(expected);
            result.Value.Should().BeNull();
            result.Error.Should().Be("e");
        }
    }
}
