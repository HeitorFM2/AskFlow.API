using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Handlers;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class GetPostsByUserNameHandlerTests
    {
        private readonly IPostQueries _postQueries = Substitute.For<IPostQueries>();
        private readonly ILikeQueries _likeQueries = Substitute.For<ILikeQueries>();
        private readonly UserManager<User> _userManager = UserManagerFixture.Create();

        private GetPostsByUserNameHandler CreateSut(string? userId) =>
            new(_postQueries, _likeQueries,
                userId is null
                    ? HttpContextFixture.CreateUnauthenticated()
                    : HttpContextFixture.CreateAuthenticated(userId),
                _userManager);

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var result = await CreateSut(null).Handle(new GetPostsByUserNameQuery("alice"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
        }

        [Fact]
        public async Task Handle_TargetUserNotFound_ShouldReturnNotFound()
        {
            _userManager.FindByNameAsync("ghost").Returns((User?)null);

            var result = await CreateSut("u1").Handle(new GetPostsByUserNameQuery("ghost"), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotFound);
            await _postQueries.DidNotReceive().GetByUserAsync(
                Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_EmptyPosts_ShouldReturnEmptyPagedResult_AndSkipLikeQuery()
        {
            var target = new UserBuilder().Build();
            _userManager.FindByNameAsync(target.UserName!).Returns(target);
            _postQueries.CountByUserAsync(target.Id, Arg.Any<CancellationToken>()).Returns(0);
            _postQueries.GetByUserAsync(target.Id, 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<PostsViewModel>());

            var result = await CreateSut("u1").Handle(new GetPostsByUserNameQuery(target.UserName!, 1, 20), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
            await _likeQueries.DidNotReceive().GetLikedPostIdsAsync(
                Arg.Any<string>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldFlag_IsLiked_ForKnownPosts()
        {
            const string userId = "u1";
            var target = new UserBuilder().Build();
            var posts = new List<PostsViewModel>
            {
                new() { Id = 1, User = new UserDto { UserName = target.UserName! } },
                new() { Id = 2, User = new UserDto { UserName = target.UserName! } }
            };
            _userManager.FindByNameAsync(target.UserName!).Returns(target);
            _postQueries.CountByUserAsync(target.Id, Arg.Any<CancellationToken>()).Returns(2);
            _postQueries.GetByUserAsync(target.Id, 1, 20, Arg.Any<CancellationToken>()).Returns(posts);
            _likeQueries.GetLikedPostIdsAsync(userId, Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
                .Returns(new HashSet<int> { 2 });

            var result = await CreateSut(userId).Handle(new GetPostsByUserNameQuery(target.UserName!, 1, 20), default);

            result.Value!.Items.Single(p => p.Id == 1).IsLiked.Should().BeFalse();
            result.Value.Items.Single(p => p.Id == 2).IsLiked.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldReturnPagedResult_WithCorrectPaginationFields()
        {
            const string userId = "u1";
            var target = new UserBuilder().Build();
            _userManager.FindByNameAsync(target.UserName!).Returns(target);
            _postQueries.CountByUserAsync(target.Id, Arg.Any<CancellationToken>()).Returns(50);
            _postQueries.GetByUserAsync(target.Id, 3, 5, Arg.Any<CancellationToken>())
                .Returns(new List<PostsViewModel>());

            var result = await CreateSut(userId).Handle(new GetPostsByUserNameQuery(target.UserName!, 3, 5), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalCount.Should().Be(50);
            result.Value.Page.Should().Be(3);
            result.Value.PageSize.Should().Be(5);
        }
    }
}
