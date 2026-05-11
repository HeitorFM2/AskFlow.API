using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Command;
using AskFlow.Application.Posts.Handlers;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class CreatePostHandlerTests
    {
        private readonly IPostRepository _repository = Substitute.For<IPostRepository>();
        private readonly ILogger<CreatePostHandler> _logger = Substitute.For<ILogger<CreatePostHandler>>();

        [Fact]
        public async Task Handle_NoUserClaim_ShouldReturnUnauthorized()
        {
            var sut = new CreatePostHandler(_repository, HttpContextFixture.CreateUnauthenticated(), _logger);

            var result = await sut.Handle(new CreatePostCommand("texto"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _repository.DidNotReceive().AddAsync(Arg.Any<Post>());
        }

        [Fact]
        public async Task Handle_NullHttpContext_ShouldReturnUnauthorized()
        {
            var sut = new CreatePostHandler(_repository, HttpContextFixture.CreateNullContext(), _logger);

            var result = await sut.Handle(new CreatePostCommand("texto"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldPersistPost_AndReturnId()
        {
            var sut = new CreatePostHandler(_repository, HttpContextFixture.CreateAuthenticated("user-99"), _logger);
            _ = _repository.AddAsync(Arg.Do<Post>(p => p.Id = 123));

            var result = await sut.Handle(new CreatePostCommand("texto"), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(123);
            await _repository.Received(1).AddAsync(Arg.Is<Post>(p => p.UserId == "user-99" && p.Content == "texto"));
        }

        [Fact]
        public async Task Handle_AuthenticatedViaSubClaim_ShouldAlsoWork()
        {
            var sut = new CreatePostHandler(_repository, HttpContextFixture.CreateAuthenticated("user-1", claimType: "sub"), _logger);

            var result = await sut.Handle(new CreatePostCommand("conteudo"), default);

            result.IsSuccess.Should().BeTrue();
            await _repository.Received(1).AddAsync(Arg.Is<Post>(p => p.UserId == "user-1"));
        }
    }
}
