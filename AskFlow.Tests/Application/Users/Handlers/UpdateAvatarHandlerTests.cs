using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.Commands;
using AskFlow.Application.Users.Handlers;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Application.Users.Handlers
{
    public class UpdateAvatarHandlerTests
    {
        private readonly UserManager<User> _userManager = UserManagerFixture.Create();
        private readonly IAvatarStorage _avatarStorage = Substitute.For<IAvatarStorage>();
        private readonly IImageProcessor _imageProcessor = Substitute.For<IImageProcessor>();
        private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
        private readonly ILogger<UpdateAvatarHandler> _logger = Substitute.For<ILogger<UpdateAvatarHandler>>();

        private UpdateAvatarHandler CreateSut() =>
            new(_userManager, _avatarStorage, _imageProcessor, _currentUser, _logger);

        private static UpdateAvatarCommand Command() =>
            new(new MemoryStream([1, 2, 3]), "image/jpeg", 3);

        private static ProcessedImage ValidProcessedImage() =>
            new(new MemoryStream([0xFF, 0xD8, 0xFF]), "image/webp");

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            _currentUser.GetUserId().Returns((string?)null);

            var result = await CreateSut().Handle(Command(), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _avatarStorage.DidNotReceive().UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_UserNotFound_ShouldReturnNotFound()
        {
            _currentUser.GetUserId().Returns("u1");
            _userManager.FindByIdAsync("u1").Returns((User?)null);

            var result = await CreateSut().Handle(Command(), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotFound);
            await _avatarStorage.DidNotReceive().UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_InvalidImage_ShouldReturnInvalid()
        {
            var user = new UserBuilder().WithId("u1").Build();
            _currentUser.GetUserId().Returns("u1");
            _userManager.FindByIdAsync("u1").Returns(user);
            _imageProcessor.ProcessAvatarAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns((ProcessedImage?)null);

            var result = await CreateSut().Handle(Command(), default);

            result.Type.Should().Be(ResultType.Invalid);
            result.ErrorCode.Should().Be(ErrorCodes.AvatarInvalidContent);
            await _avatarStorage.DidNotReceive().UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_HappyPath_ShouldDeleteOld_UploadNew_PersistUrl_ReturnSuccess()
        {
            var user = new UserBuilder().WithId("u1").Build();
            _currentUser.GetUserId().Returns("u1");
            _userManager.FindByIdAsync("u1").Returns(user);
            _imageProcessor.ProcessAvatarAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(ValidProcessedImage());
            _avatarStorage.UploadAsync(Arg.Any<Stream>(), "image/webp", "u1", Arg.Any<CancellationToken>())
                .Returns("https://blob/u1.webp");
            _userManager.UpdateAsync(user).Returns(IdentityResult.Success);

            var result = await CreateSut().Handle(Command(), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("https://blob/u1.webp");
            user.AvatarUrl.Should().Be("https://blob/u1.webp");
            Received.InOrder(async () =>
            {
                await _avatarStorage.DeleteAsync("u1", Arg.Any<CancellationToken>());
                await _avatarStorage.UploadAsync(Arg.Any<Stream>(), "image/webp", "u1", Arg.Any<CancellationToken>());
                await _userManager.UpdateAsync(user);
            });
        }

        [Fact]
        public async Task Handle_UpdateAsyncFails_ShouldReturnFailure_WithJoinedErrors()
        {
            var user = new UserBuilder().WithId("u1").Build();
            _currentUser.GetUserId().Returns("u1");
            _userManager.FindByIdAsync("u1").Returns(user);
            _imageProcessor.ProcessAvatarAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(ValidProcessedImage());
            _avatarStorage.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns("https://blob/u1.webp");
            _userManager.UpdateAsync(user).Returns(IdentityResult.Failed(
                new IdentityError { Description = "boom1" },
                new IdentityError { Description = "boom2" }));

            var result = await CreateSut().Handle(Command(), default);

            result.Type.Should().Be(ResultType.Failure);
            result.ErrorCode.Should().Be(ErrorCodes.AvatarUploadFailed);
            result.Error.Should().Contain("boom1").And.Contain("boom2");
        }
    }
}
