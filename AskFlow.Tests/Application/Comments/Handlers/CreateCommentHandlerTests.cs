using AskFlow.Application.Comments.Commands;
using AskFlow.Application.Comments.Handlers;
using AskFlow.Application.Common;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Application.Comments.Handlers
{
    public class CreateCommentHandlerTests
    {
        private readonly ICommentRepository _comments = Substitute.For<ICommentRepository>();
        private readonly IPostRepository _posts = Substitute.For<IPostRepository>();
        private readonly ILogger<CreateCommentHandler> _logger = Substitute.For<ILogger<CreateCommentHandler>>();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new CreateCommentHandler(_comments, _posts, HttpContextFixture.CreateUnauthenticated(), _logger);

            var result = await sut.Handle(new CreateCommentCommand(1, "x"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
        }

        [Fact]
        public async Task Handle_PostNotFound_ShouldReturnNotFound()
        {
            _posts.GetByIdAsync(1).Returns((Post?)null);
            var sut = new CreateCommentHandler(_comments, _posts, HttpContextFixture.CreateAuthenticated("u1"), _logger);

            var result = await sut.Handle(new CreateCommentCommand(1, "x"), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.PostNotFound);
        }

        [Fact]
        public async Task Handle_ParentMissing_ShouldReturnNotFound()
        {
            var post = new PostBuilder().WithId(1).Build();
            _posts.GetByIdAsync(1).Returns(post);
            _comments.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Comment?)null);
            var sut = new CreateCommentHandler(_comments, _posts, HttpContextFixture.CreateAuthenticated("u1"), _logger);

            var result = await sut.Handle(new CreateCommentCommand(1, "x", ParentCommentId: 99), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.CommentParentNotFound);
        }

        [Fact]
        public async Task Handle_RootComment_ShouldPersist_AndReturnId()
        {
            var post = new PostBuilder().WithId(1).Build();
            _posts.GetByIdAsync(1).Returns(post);
            _ = _comments.AddAsync(Arg.Do<Comment>(c => c.Id = 55), Arg.Any<CancellationToken>());
            var sut = new CreateCommentHandler(_comments, _posts, HttpContextFixture.CreateAuthenticated("u1"), _logger);

            var result = await sut.Handle(new CreateCommentCommand(1, "olá"), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(55);
            await _comments.Received(1).AddAsync(
                Arg.Is<Comment>(c => c.UserId == "u1" && c.PostId == 1 && c.ParentCommentId == null && c.Content == "olá"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Reply_ShouldPersistWithParent()
        {
            var post = new PostBuilder().WithId(1).Build();
            var parent = new CommentBuilder().WithId(99).WithPostId(1).Build();
            _posts.GetByIdAsync(1).Returns(post);
            _comments.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns(parent);
            _ = _comments.AddAsync(Arg.Do<Comment>(c => c.Id = 100), Arg.Any<CancellationToken>());
            var sut = new CreateCommentHandler(_comments, _posts, HttpContextFixture.CreateAuthenticated("u1"), _logger);

            var result = await sut.Handle(new CreateCommentCommand(1, "reply", ParentCommentId: 99), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(100);
            await _comments.Received(1).AddAsync(
                Arg.Is<Comment>(c => c.ParentCommentId == 99),
                Arg.Any<CancellationToken>());
        }
    }
}
