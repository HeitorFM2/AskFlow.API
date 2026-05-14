using AskFlow.Application.Comments.Commands;
using AskFlow.Application.Comments.Handlers;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Comments.Handlers
{
    public class DeleteCommentHandlerTests
    {
        private readonly ICommentRepository _repo = Substitute.For<ICommentRepository>();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new DeleteCommentHandler(_repo, HttpContextFixture.CreateUnauthenticated());

            var result = await sut.Handle(new DeleteCommentCommand(1), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
        }

        [Fact]
        public async Task Handle_NotFound_ShouldReturnNotFound()
        {
            _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Comment?)null);
            var sut = new DeleteCommentHandler(_repo, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new DeleteCommentCommand(1), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.CommentNotFound);
        }

        [Fact]
        public async Task Handle_OwnedByOtherUser_ShouldReturnForbidden()
        {
            var c = new CommentBuilder().WithUserId("other").Build();
            _repo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(c);
            var sut = new DeleteCommentHandler(_repo, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new DeleteCommentCommand(1), default);

            result.Type.Should().Be(ResultType.Forbidden);
            result.ErrorCode.Should().Be(ErrorCodes.CommentNoPermissionToDelete);
            await _repo.DidNotReceive().DeleteAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Owner_ShouldDelete()
        {
            var c = new CommentBuilder().WithUserId("u1").Build();
            _repo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(c);
            var sut = new DeleteCommentHandler(_repo, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new DeleteCommentCommand(1), default);

            result.Type.Should().Be(ResultType.Ok);
            await _repo.Received(1).DeleteAsync(c, Arg.Any<CancellationToken>());
        }
    }
}
