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
        private readonly ICommentRepository _comments = Substitute.For<ICommentRepository>();
        private readonly IPostRepository _posts = Substitute.For<IPostRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new DeleteCommentHandler(_comments, _posts, _unitOfWork, HttpContextFixture.CreateUnauthenticated());

            var result = await sut.Handle(new DeleteCommentCommand(1), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
        }

        [Fact]
        public async Task Handle_NotFound_ShouldReturnNotFound()
        {
            _comments.FindByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Comment?)null);
            var sut = new DeleteCommentHandler(_comments, _posts, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new DeleteCommentCommand(1), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.CommentNotFound);
        }

        [Fact]
        public async Task Handle_OwnedByOtherUser_ShouldReturnForbidden()
        {
            var c = new CommentBuilder().WithUserId("other").Build();
            _comments.FindByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(c);
            var sut = new DeleteCommentHandler(_comments, _posts, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new DeleteCommentCommand(1), default);

            result.Type.Should().Be(ResultType.Forbidden);
            result.ErrorCode.Should().Be(ErrorCodes.CommentNoPermissionToDelete);
            _comments.DidNotReceive().Delete(Arg.Any<Comment>());
        }

        [Fact]
        public async Task Handle_Owner_ShouldDelete_AndDecrementPostCounter()
        {
            var post = new PostBuilder().WithId(7).Build();
            post.IncrementCommentCount();
            var c = new CommentBuilder().WithPostId(7).WithUserId("u1").Build();
            _comments.FindByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(c);
            _posts.FindByIdAsync(7, Arg.Any<CancellationToken>()).Returns(post);
            var sut = new DeleteCommentHandler(_comments, _posts, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new DeleteCommentCommand(1), default);

            result.Type.Should().Be(ResultType.Ok);
            _comments.Received(1).Delete(c);
            post.CommentCount.Should().Be(0);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Owner_OrphanComment_ShouldDelete_WithoutTouchingPost()
        {
            var c = new CommentBuilder().WithUserId("u1").Build();
            c.PostId = null;
            _comments.FindByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(c);
            var sut = new DeleteCommentHandler(_comments, _posts, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new DeleteCommentCommand(1), default);

            result.Type.Should().Be(ResultType.Ok);
            _comments.Received(1).Delete(c);
            await _posts.DidNotReceive().FindByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        }
    }
}
