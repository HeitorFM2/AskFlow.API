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
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly ILogger<CreatePostHandler> _logger = Substitute.For<ILogger<CreatePostHandler>>();

        [Fact]
        public async Task Handle_NoUserClaim_ShouldReturnUnauthorized()
        {
            var sut = new CreatePostHandler(_repository, _unitOfWork, HttpContextFixture.CreateUnauthenticated(), _logger);

            var result = await sut.Handle(new CreatePostCommand("texto"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            _repository.DidNotReceive().Add(Arg.Any<Post>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_NullHttpContext_ShouldReturnUnauthorized()
        {
            var sut = new CreatePostHandler(_repository, _unitOfWork, HttpContextFixture.CreateNullContext(), _logger);

            var result = await sut.Handle(new CreatePostCommand("texto"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldPersistPost_AndReturnId()
        {
            var sut = new CreatePostHandler(_repository, _unitOfWork, HttpContextFixture.CreateAuthenticated("user-99"), _logger);
            _repository.When(r => r.Add(Arg.Any<Post>())).Do(c => c.Arg<Post>().Id = 123);

            var result = await sut.Handle(new CreatePostCommand("texto"), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(123);
            _repository.Received(1).Add(Arg.Is<Post>(p => p.UserId == "user-99" && p.Content == "texto"));
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_AuthenticatedViaSubClaim_ShouldAlsoWork()
        {
            var sut = new CreatePostHandler(_repository, _unitOfWork, HttpContextFixture.CreateAuthenticated("user-1"), _logger);

            var result = await sut.Handle(new CreatePostCommand("conteudo"), default);

            result.IsSuccess.Should().BeTrue();
            _repository.Received(1).Add(Arg.Is<Post>(p => p.UserId == "user-1"));
        }
    }
}
