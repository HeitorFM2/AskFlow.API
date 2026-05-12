using AskFlow.Application.Common;
using AskFlow.Application.Users.Commands;
using AskFlow.Application.Users.Queries;
using AskFlow.Application.Users.ViewModels;
using AskFlow.WebAPI.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AskFlow.Tests.WebAPI.Controllers
{
    public class UsersControllerTests
    {
        private readonly IMediator _mediator = Substitute.For<IMediator>();
        private UsersController CreateSut() => new(_mediator);

        private static IFormFile FakeFile(string contentType = "image/jpeg", long length = 1024)
        {
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(length);
            file.ContentType.Returns(contentType);
            file.OpenReadStream().Returns(new MemoryStream([1, 2, 3]));
            return file;
        }

        [Fact]
        public async Task UpdateAvatar_NullFile_ShouldReturnBadRequest_WithAvatarRequired()
        {
            var action = await CreateSut().UpdateAvatar(null!);

            var bad = action.Should().BeOfType<BadRequestObjectResult>().Subject;
            bad.Value.Should().BeEquivalentTo(new { code = ErrorCodes.AvatarRequired, message = "Avatar file is required." });
            await _mediator.DidNotReceive().Send(Arg.Any<UpdateAvatarCommand>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UpdateAvatar_ZeroLengthFile_ShouldReturnBadRequest_WithAvatarRequired()
        {
            var action = await CreateSut().UpdateAvatar(FakeFile(length: 0));

            action.Should().BeOfType<BadRequestObjectResult>();
            await _mediator.DidNotReceive().Send(Arg.Any<UpdateAvatarCommand>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UpdateAvatar_ValidFile_ShouldDispatchCommand_AndReturnOk()
        {
            _mediator.Send(Arg.Any<UpdateAvatarCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<string>.Success("https://blob/u1.jpg"));

            var file = FakeFile("image/png", 512);

            var action = await CreateSut().UpdateAvatar(file);

            action.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be("https://blob/u1.jpg");
            await _mediator.Received(1).Send(
                Arg.Is<UpdateAvatarCommand>(c => c.ContentType == "image/png" && c.Length == 512),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UpdateAvatar_HandlerFailure_ShouldMapToActionResult()
        {
            _mediator.Send(Arg.Any<UpdateAvatarCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<string>.NotFound(ErrorCodes.UserNotFound, "User not found."));

            var action = await CreateSut().UpdateAvatar(FakeFile());

            action.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetMe_HappyPath_ShouldReturnOk_WithUserDto()
        {
            var dto = new UserDto { UserName = "alice", Identification = "alice", AvatarUrl = "https://blob/u1.jpg" };
            _mediator.Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<UserDto>.Success(dto));

            var action = await CreateSut().GetMe();

            action.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetMe_HandlerFailure_ShouldMapToActionResult()
        {
            _mediator.Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<UserDto>.NotFound(ErrorCodes.UserNotFound, "User not found."));

            var action = await CreateSut().GetMe();

            action.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
