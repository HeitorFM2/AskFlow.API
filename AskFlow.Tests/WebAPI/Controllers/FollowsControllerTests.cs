using AskFlow.Application.Common;
using AskFlow.Application.Follows.Commands;
using AskFlow.Application.Follows.Queries;
using AskFlow.Application.Follows.ViewModels;
using AskFlow.WebAPI.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AskFlow.Tests.WebAPI.Controllers
{
    public class FollowsControllerTests
    {
        private readonly IMediator _mediator = Substitute.For<IMediator>();
        private FollowsController CreateSut() => new(_mediator);

        // --- ToggleFollow ---

        [Fact]
        public async Task ToggleFollow_OnFollow_ShouldReturnCreatedAtAction_WithFollowingTrue()
        {
            _mediator.Send(Arg.Any<ToggleFollowCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            var action = await CreateSut().ToggleFollow(new ToggleFollowCommand("alice"));

            var created = action.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.ActionName.Should().Be(nameof(FollowsController.ToggleFollow));
            created.RouteValues!["TargetUserName"].Should().Be("alice");
            created.Value.Should().BeEquivalentTo(new { following = true });
        }

        [Fact]
        public async Task ToggleFollow_OnUnfollow_ShouldReturnCreatedAtAction_WithFollowingFalse()
        {
            _mediator.Send(Arg.Any<ToggleFollowCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(false));

            var action = await CreateSut().ToggleFollow(new ToggleFollowCommand("alice"));

            var created = action.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.Value.Should().BeEquivalentTo(new { following = false });
        }

        [Fact]
        public async Task ToggleFollow_OnUnauthorized_ShouldReturnUnauthorized()
        {
            _mediator.Send(Arg.Any<ToggleFollowCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated."));

            var action = await CreateSut().ToggleFollow(new ToggleFollowCommand("alice"));

            action.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task ToggleFollow_OnNotFound_ShouldReturnNotFound()
        {
            _mediator.Send(Arg.Any<ToggleFollowCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.NotFound(ErrorCodes.UserNotFound, "User not found."));

            var action = await CreateSut().ToggleFollow(new ToggleFollowCommand("ghost"));

            action.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task ToggleFollow_OnInvalid_ShouldReturnBadRequest()
        {
            _mediator.Send(Arg.Any<ToggleFollowCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Invalid(ErrorCodes.FollowCannotFollowSelf, "Cannot follow yourself."));

            var action = await CreateSut().ToggleFollow(new ToggleFollowCommand("me"));

            action.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- GetFollowers ---

        [Fact]
        public async Task GetFollowers_ShouldReturnOk_OnSuccess()
        {
            _mediator.Send(Arg.Any<GetFollowersQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<FollowViewModel>>.Success(new PagedResult<FollowViewModel>()));

            var action = await CreateSut().GetFollowers();

            action.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetFollowers_ShouldReturnUnauthorized_OnUnauthorized()
        {
            _mediator.Send(Arg.Any<GetFollowersQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<FollowViewModel>>.Unauthorized(
                    ErrorCodes.UserNotAuthenticated, "User not authenticated."));

            var action = await CreateSut().GetFollowers();

            action.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetFollowers_ShouldForwardPageAndSearchParams_ToQuery()
        {
            _mediator.Send(Arg.Any<GetFollowersQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<FollowViewModel>>.Success(new PagedResult<FollowViewModel>()));

            await CreateSut().GetFollowers(page: 2, pageSize: 10, search: "alice");

            await _mediator.Received(1).Send(
                Arg.Is<GetFollowersQuery>(q => q.Page == 2 && q.PageSize == 10 && q.Search == "alice"),
                Arg.Any<CancellationToken>());
        }

        // --- GetFollowing ---

        [Fact]
        public async Task GetFollowing_ShouldReturnOk_OnSuccess()
        {
            _mediator.Send(Arg.Any<GetFollowingQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<FollowViewModel>>.Success(new PagedResult<FollowViewModel>()));

            var action = await CreateSut().GetFollowing();

            action.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetFollowing_ShouldReturnUnauthorized_OnUnauthorized()
        {
            _mediator.Send(Arg.Any<GetFollowingQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<FollowViewModel>>.Unauthorized(
                    ErrorCodes.UserNotAuthenticated, "User not authenticated."));

            var action = await CreateSut().GetFollowing();

            action.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetFollowing_ShouldForwardPageAndSearchParams_ToQuery()
        {
            _mediator.Send(Arg.Any<GetFollowingQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<FollowViewModel>>.Success(new PagedResult<FollowViewModel>()));

            await CreateSut().GetFollowing(page: 3, pageSize: 5, search: "bob");

            await _mediator.Received(1).Send(
                Arg.Is<GetFollowingQuery>(q => q.Page == 3 && q.PageSize == 5 && q.Search == "bob"),
                Arg.Any<CancellationToken>());
        }

        // --- GetStats ---

        [Fact]
        public async Task GetStats_ShouldReturnOk_OnSuccess()
        {
            _mediator.Send(Arg.Any<GetFollowStatsQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<FollowStatsViewModel>.Success(new FollowStatsViewModel { Followers = 3, Following = 7 }));

            var action = await CreateSut().GetStats();

            action.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStats_ShouldReturnUnauthorized_OnUnauthorized()
        {
            _mediator.Send(Arg.Any<GetFollowStatsQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<FollowStatsViewModel>.Unauthorized(
                    ErrorCodes.UserNotAuthenticated, "User not authenticated."));

            var action = await CreateSut().GetStats();

            action.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}
