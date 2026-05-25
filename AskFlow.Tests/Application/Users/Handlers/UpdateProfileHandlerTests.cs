using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.Commands;
using AskFlow.Application.Users.Handlers;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Tests.Application.Users.Handlers
{
    public class UpdateProfileHandlerTests
    {
        private readonly UserManager<User> _userManager = UserManagerFixture.Create();
        private readonly IUserQueries _userQueries = Substitute.For<IUserQueries>();

        private UpdateProfileHandler CreateSut(string? userId) =>
            new(_userManager, _userQueries,
                userId is null
                    ? HttpContextFixture.CreateUnauthenticated()
                    : HttpContextFixture.CreateAuthenticated(userId));

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var result = await CreateSut(null).Handle(new UpdateProfileCommand("name", "id"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
        }

        [Fact]
        public async Task Handle_UserNotFound_ShouldReturnNotFound()
        {
            _userManager.FindByIdAsync("u1").Returns((User?)null);

            var result = await CreateSut("u1").Handle(new UpdateProfileCommand("name", "id"), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotFound);
        }

        [Fact]
        public async Task Handle_UserNameTakenByAnotherUser_ShouldReturnConflict()
        {
            var user = new UserBuilder().WithId("u1").Build();
            var other = new UserBuilder().WithId("u2").Build();
            _userManager.FindByIdAsync("u1").Returns(user);
            _userManager.FindByNameAsync("taken").Returns(other);

            var result = await CreateSut("u1").Handle(new UpdateProfileCommand("taken", "my_id"), default);

            result.Type.Should().Be(ResultType.Conflict);
            result.ErrorCode.Should().Be(ErrorCodes.UserNameTaken);
        }

        [Fact]
        public async Task Handle_UserNameOwnedBySameUser_ShouldNotConflict()
        {
            var user = new UserBuilder().WithId("u1").Build();
            _userManager.FindByIdAsync("u1").Returns(user);
            _userManager.FindByNameAsync(user.UserName!).Returns(user);
            _userQueries.IsIdentificationTakenAsync(Arg.Any<string>(), "u1", Arg.Any<CancellationToken>()).Returns(false);
            _userManager.UpdateAsync(user).Returns(IdentityResult.Success);

            var result = await CreateSut("u1").Handle(new UpdateProfileCommand(user.UserName!, "new_id"), default);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_IdentificationTaken_ShouldReturnConflict()
        {
            var user = new UserBuilder().WithId("u1").Build();
            _userManager.FindByIdAsync("u1").Returns(user);
            _userManager.FindByNameAsync(Arg.Any<string>()).Returns((User?)null);
            _userQueries.IsIdentificationTakenAsync("taken_id", "u1", Arg.Any<CancellationToken>()).Returns(true);

            var result = await CreateSut("u1").Handle(new UpdateProfileCommand("newname", "taken_id"), default);

            result.Type.Should().Be(ResultType.Conflict);
            result.ErrorCode.Should().Be(ErrorCodes.IdentificationTaken);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldUpdateAndReturnDto()
        {
            var user = new UserBuilder().WithId("u1").Build();
            _userManager.FindByIdAsync("u1").Returns(user);
            _userManager.FindByNameAsync("newname").Returns((User?)null);
            _userQueries.IsIdentificationTakenAsync("new_id", "u1", Arg.Any<CancellationToken>()).Returns(false);
            _userManager.UpdateAsync(user).Returns(IdentityResult.Success);

            var result = await CreateSut("u1").Handle(new UpdateProfileCommand("newname", "new_id"), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.UserName.Should().Be("newname");
            result.Value.Identification.Should().Be("new_id");
        }

        [Fact]
        public async Task Handle_IdentityFailure_ShouldReturnFailure()
        {
            var user = new UserBuilder().WithId("u1").Build();
            _userManager.FindByIdAsync("u1").Returns(user);
            _userManager.FindByNameAsync(Arg.Any<string>()).Returns((User?)null);
            _userQueries.IsIdentificationTakenAsync(Arg.Any<string>(), "u1", Arg.Any<CancellationToken>()).Returns(false);
            _userManager.UpdateAsync(user).Returns(IdentityResult.Failed(new IdentityError { Description = "DB error" }));

            var result = await CreateSut("u1").Handle(new UpdateProfileCommand("newname", "new_id"), default);

            result.IsSuccess.Should().BeFalse();
            result.Type.Should().Be(ResultType.Failure);
            result.ErrorCode.Should().Be(ErrorCodes.AuthIdentityFailure);
        }
    }
}
