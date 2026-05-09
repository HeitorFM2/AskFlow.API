using AskFlow.Application.Common;
using AskFlow.Application.Posts.Command;
using AskFlow.Application.Posts.Handlers;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class DeletePostHandlerTests
    {
        private readonly IPostRepository _repository = Substitute.For<IPostRepository>();
        private readonly ILogger<DeletePostHandler> _logger = Substitute.For<ILogger<DeletePostHandler>>();

        [Fact]
        public async Task Handle_NoUser_ShouldReturnUnauthorized()
        {
            var sut = new DeletePostHandler(_repository, HttpContextFixture.CreateUnauthenticated(), _logger);

            var result = await sut.Handle(new DeletePostCommand(1), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            await _repository.DidNotReceive().DeleteAsync(Arg.Any<Post>());
        }

        [Fact]
        public async Task Handle_PostNotFound_ShouldReturnNotFound()
        {
            _repository.GetByIdAsync(1).Returns((Post?)null);
            var sut = new DeletePostHandler(_repository, HttpContextFixture.CreateAuthenticated("user-1"), _logger);

            var result = await sut.Handle(new DeletePostCommand(1), default);

            result.Type.Should().Be(ResultType.NotFound);
        }

        [Fact]
        public async Task Handle_PostFromOtherUser_ShouldReturnUnauthorized()
        {
            var post = new PostBuilder().WithId(5).WithUserId("other-user").Build();
            _repository.GetByIdAsync(5).Returns(post);
            var sut = new DeletePostHandler(_repository, HttpContextFixture.CreateAuthenticated("user-1"), _logger);

            var result = await sut.Handle(new DeletePostCommand(5), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            await _repository.DidNotReceive().DeleteAsync(Arg.Any<Post>());
        }

        [Fact]
        public async Task Handle_Owner_ShouldDeletePost()
        {
            var post = new PostBuilder().WithId(7).WithUserId("user-1").Build();
            _repository.GetByIdAsync(7).Returns(post);
            var sut = new DeletePostHandler(_repository, HttpContextFixture.CreateAuthenticated("user-1"), _logger);

            var result = await sut.Handle(new DeletePostCommand(7), default);

            result.Type.Should().Be(ResultType.Ok);
            await _repository.Received(1).DeleteAsync(post);
        }
    }
}
