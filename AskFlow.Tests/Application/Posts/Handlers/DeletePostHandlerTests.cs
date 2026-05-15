using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Command;
using AskFlow.Application.Posts.Handlers;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class DeletePostHandlerTests
    {
        private readonly IPostRepository _repository = Substitute.For<IPostRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly ILogger<DeletePostHandler> _logger = Substitute.For<ILogger<DeletePostHandler>>();

        [Fact]
        public async Task Handle_NoUser_ShouldReturnUnauthorized()
        {
            var sut = new DeletePostHandler(_repository, _unitOfWork, HttpContextFixture.CreateUnauthenticated(), _logger);

            var result = await sut.Handle(new DeletePostCommand(1), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            _repository.DidNotReceive().Delete(Arg.Any<Post>());
        }

        [Fact]
        public async Task Handle_PostNotFound_ShouldReturnNotFound()
        {
            _repository.FindByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Post?)null);
            var sut = new DeletePostHandler(_repository, _unitOfWork, HttpContextFixture.CreateAuthenticated("user-1"), _logger);

            var result = await sut.Handle(new DeletePostCommand(1), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.PostNotFound);
        }

        [Fact]
        public async Task Handle_PostFromOtherUser_ShouldReturnForbidden()
        {
            var post = new PostBuilder().WithId(5).WithUserId("other-user").Build();
            _repository.FindByIdAsync(5, Arg.Any<CancellationToken>()).Returns(post);
            var sut = new DeletePostHandler(_repository, _unitOfWork, HttpContextFixture.CreateAuthenticated("user-1"), _logger);

            var result = await sut.Handle(new DeletePostCommand(5), default);

            result.Type.Should().Be(ResultType.Forbidden);
            result.ErrorCode.Should().Be(ErrorCodes.PostNoPermissionToDelete);
            _repository.DidNotReceive().Delete(Arg.Any<Post>());
        }

        [Fact]
        public async Task Handle_Owner_ShouldDeletePost()
        {
            var post = new PostBuilder().WithId(7).WithUserId("user-1").Build();
            _repository.FindByIdAsync(7, Arg.Any<CancellationToken>()).Returns(post);
            var sut = new DeletePostHandler(_repository, _unitOfWork, HttpContextFixture.CreateAuthenticated("user-1"), _logger);

            var result = await sut.Handle(new DeletePostCommand(7), default);

            result.Type.Should().Be(ResultType.Ok);
            _repository.Received(1).Delete(post);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
