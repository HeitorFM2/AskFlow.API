using AskFlow.Application.Comments.Commands;
using AskFlow.Application.Comments.Handlers;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Application.Comments.Handlers
{
    public class CreateCommentHandlerTests
    {
        private readonly ICommentRepository _comments = Substitute.For<ICommentRepository>();
        private readonly IPostRepository _posts = Substitute.For<IPostRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly ILogger<CreateCommentHandler> _logger = Substitute.For<ILogger<CreateCommentHandler>>();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new CreateCommentHandler(_comments, _posts, _unitOfWork, HttpContextFixture.CreateUnauthenticated(), _logger);

            var result = await sut.Handle(new CreateCommentCommand(1, "x"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
        }

        [Fact]
        public async Task Handle_PostNotFound_ShouldReturnNotFound()
        {
            _posts.FindByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Post?)null);
            var sut = new CreateCommentHandler(_comments, _posts, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"), _logger);

            var result = await sut.Handle(new CreateCommentCommand(1, "x"), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.PostNotFound);
        }

        [Fact]
        public async Task Handle_ParentMissing_ShouldReturnNotFound()
        {
            _posts.FindByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new PostBuilder().WithId(1).Build());
            _comments.FindByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Comment?)null);
            var sut = new CreateCommentHandler(_comments, _posts, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"), _logger);

            var result = await sut.Handle(new CreateCommentCommand(1, "x", ParentCommentId: 99), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.CommentParentNotFound);
        }

        [Fact]
        public async Task Handle_RootComment_ShouldPersist_IncrementCounter_AndReturnId()
        {
            var post = new PostBuilder().WithId(1).Build();
            _posts.FindByIdAsync(1, Arg.Any<CancellationToken>()).Returns(post);
            _comments.When(c => c.Add(Arg.Any<Comment>())).Do(c => c.Arg<Comment>().Id = 55);
            var sut = new CreateCommentHandler(_comments, _posts, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"), _logger);

            var result = await sut.Handle(new CreateCommentCommand(1, "olá"), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(55);
            _comments.Received(1).Add(Arg.Is<Comment>(c => c.UserId == "u1" && c.PostId == 1 && c.ParentCommentId == null && c.Content == "olá"));
            post.CommentCount.Should().Be(1);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Reply_ShouldPersistWithParent()
        {
            var post = new PostBuilder().WithId(1).Build();
            var parent = new CommentBuilder().WithId(99).WithPostId(1).Build();
            _posts.FindByIdAsync(1, Arg.Any<CancellationToken>()).Returns(post);
            _comments.FindByIdAsync(99, Arg.Any<CancellationToken>()).Returns(parent);
            _comments.When(c => c.Add(Arg.Any<Comment>())).Do(c => c.Arg<Comment>().Id = 100);
            var sut = new CreateCommentHandler(_comments, _posts, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"), _logger);

            var result = await sut.Handle(new CreateCommentCommand(1, "reply", ParentCommentId: 99), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(100);
            _comments.Received(1).Add(Arg.Is<Comment>(c => c.ParentCommentId == 99));
        }
    }
}
