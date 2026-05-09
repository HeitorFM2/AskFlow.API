using AskFlow.Application.Common.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AskFlow.Tests.Application.Common
{
    public class ValidationBehaviorTests
    {
        public record FakeRequest(string Value) : IRequest<string>;

        [Fact]
        public async Task Handle_WithoutValidators_ShouldCallNext()
        {
            var behavior = new ValidationBehavior<FakeRequest, string>(Array.Empty<IValidator<FakeRequest>>());
            var nextCalled = false;

            var result = await behavior.Handle(
                new FakeRequest("ok"),
                (_) =>
                {
                    nextCalled = true;
                    return Task.FromResult("ok");
                },
                CancellationToken.None);

            nextCalled.Should().BeTrue();
            result.Should().Be("ok");
        }

        [Fact]
        public async Task Handle_WithValidValidators_ShouldCallNext()
        {
            var validator = Substitute.For<IValidator<FakeRequest>>();
            validator.Validate(Arg.Any<ValidationContext<FakeRequest>>())
                .Returns(new ValidationResult());

            var behavior = new ValidationBehavior<FakeRequest, string>(new[] { validator });

            var result = await behavior.Handle(
                new FakeRequest("ok"),
                (_) => Task.FromResult("ok"),
                CancellationToken.None);

            result.Should().Be("ok");
        }

        [Fact]
        public async Task Handle_WhenValidationFails_ShouldThrowValidationException()
        {
            var validator = Substitute.For<IValidator<FakeRequest>>();
            validator.Validate(Arg.Any<ValidationContext<FakeRequest>>())
                .Returns(new ValidationResult(new[]
                {
                    new ValidationFailure("Value", "obrigatório")
                }));

            var behavior = new ValidationBehavior<FakeRequest, string>(new[] { validator });

            var act = async () => await behavior.Handle(
                new FakeRequest(""),
                (_) => Task.FromResult("nope"),
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .Where(e => e.Errors.Any(f => f.ErrorMessage == "obrigatório"));
        }
    }
}
