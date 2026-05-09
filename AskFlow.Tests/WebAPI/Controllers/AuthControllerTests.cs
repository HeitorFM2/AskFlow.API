using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.ViewModels;
using AskFlow.Application.Common;
using AskFlow.WebAPI.Controllers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AskFlow.Tests.WebAPI.Controllers
{
    public class AuthControllerTests
    {
        private readonly IMediator _mediator = Substitute.For<IMediator>();

        private AuthController CreateSut(ClaimsPrincipal? user = null)
        {
            var controller = new AuthController(_mediator);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user ?? new ClaimsPrincipal(new ClaimsIdentity()) }
            };
            return controller;
        }

        private static AuthViewModel SampleViewModel() => new()
        {
            AccessToken = "a",
            RefreshToken = "r",
            ExpiresAt = DateTime.UtcNow,
            User = new UserAuthViewModel { Id = "id", Email = "e", Identification = "i" }
        };

        [Fact]
        public async Task Register_OnSuccess_ShouldReturnOk()
        {
            _mediator.Send(Arg.Any<RegisterCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<AuthViewModel>.Success(SampleViewModel()));

            var action = await CreateSut().Register(new RegisterCommand("a@b.com", "pwd", "id"));

            action.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Register_OnValidationException_ShouldReturnBadRequest()
        {
            _mediator.Send(Arg.Any<RegisterCommand>(), Arg.Any<CancellationToken>())
                .Returns<Task<Result<AuthViewModel>>>(_ => throw new ValidationException(new[] { new ValidationFailure("X", "obrig") }));

            var action = await CreateSut().Register(new RegisterCommand("", "", ""));

            action.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Login_OnSuccess_ShouldReturnOk()
        {
            _mediator.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<AuthViewModel>.Success(SampleViewModel()));

            var action = await CreateSut().Login(new LoginCommand("a@b.com", "pwd"));

            action.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Login_OnValidationException_ShouldReturnBadRequest()
        {
            _mediator.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
                .Returns<Task<Result<AuthViewModel>>>(_ => throw new ValidationException(new[] { new ValidationFailure("X", "x") }));

            var action = await CreateSut().Login(new LoginCommand("", ""));

            action.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task RefreshToken_OnSuccess_ShouldReturnOk()
        {
            _mediator.Send(Arg.Any<RefreshTokenCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<AuthViewModel>.Success(SampleViewModel()));

            var action = await CreateSut().RefreshToken(new RefreshTokenCommand("t"));

            action.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task RefreshToken_OnValidationException_ShouldReturnBadRequest()
        {
            _mediator.Send(Arg.Any<RefreshTokenCommand>(), Arg.Any<CancellationToken>())
                .Returns<Task<Result<AuthViewModel>>>(_ => throw new ValidationException(new[] { new ValidationFailure("X", "x") }));

            var action = await CreateSut().RefreshToken(new RefreshTokenCommand(""));

            action.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Logout_WithoutClaim_ShouldReturnUnauthorized()
        {
            var action = await CreateSut().Logout();
            action.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Logout_WithUserId_ShouldReturnNoContent()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-1")
            }, "TestAuth"));

            _mediator.Send(Arg.Any<LogoutCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result.Success());

            var action = await CreateSut(user).Logout();

            action.Should().BeOfType<NoContentResult>();
            await _mediator.Received(1).Send(Arg.Is<LogoutCommand>(c => c.UserId == "user-1"), Arg.Any<CancellationToken>());
        }
    }
}
