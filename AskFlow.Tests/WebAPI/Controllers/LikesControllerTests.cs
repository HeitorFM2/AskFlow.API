using AskFlow.Application.Common;
using AskFlow.Application.Likes.Commands;
using AskFlow.Application.Likes.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.WebAPI.Controllers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AskFlow.Tests.WebAPI.Controllers
{
    public class LikesControllerTests
    {
        private readonly IMediator _mediator = Substitute.For<IMediator>();
        private LikesController CreateSut() => new(_mediator);

        // GetLikedPosts

        [Fact]
        public async Task GetLikedPosts_ShouldReturnOk_OnSuccess()
        {
            _mediator.Send(Arg.Any<GetLikedPostsQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<PostsViewModel>>.Success(new PagedResult<PostsViewModel>()));

            var action = await CreateSut().GetLikedPosts();

            action.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetLikedPosts_ShouldReturnUnauthorized_OnUnauthorized()
        {
            _mediator.Send(Arg.Any<GetLikedPostsQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<PostsViewModel>>.Unauthorized(
                    ErrorCodes.UserNotAuthenticated, "User not authenticated."));

            var action = await CreateSut().GetLikedPosts();

            action.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetLikedPosts_ShouldForwardPageParams_ToQuery()
        {
            _mediator.Send(Arg.Any<GetLikedPostsQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<PostsViewModel>>.Success(new PagedResult<PostsViewModel>()));

            await CreateSut().GetLikedPosts(page: 3, pageSize: 10);

            await _mediator.Received(1).Send(
                Arg.Is<GetLikedPostsQuery>(q => q.Page == 3 && q.PageSize == 10),
                Arg.Any<CancellationToken>());
        }

        // ToggleLike

        [Fact]
        public async Task ToggleLike_OnLiked_ShouldReturnCreatedAtAction_WithLikedTrue()
        {
            _mediator.Send(Arg.Any<ToggleLikeCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            var action = await CreateSut().ToggleLike(5);

            var created = action.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.ActionName.Should().Be(nameof(LikesController.ToggleLike));
            created.RouteValues!["postId"].Should().Be(5);
            created.Value.Should().BeEquivalentTo(new { liked = true });
        }

        [Fact]
        public async Task ToggleLike_OnUnliked_ShouldReturnCreatedAtAction_WithLikedFalse()
        {
            _mediator.Send(Arg.Any<ToggleLikeCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(false));

            var action = await CreateSut().ToggleLike(5);

            var created = action.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.Value.Should().BeEquivalentTo(new { liked = false });
        }

        [Fact]
        public async Task ToggleLike_OnUnauthorized_ShouldReturnUnauthorized()
        {
            _mediator.Send(Arg.Any<ToggleLikeCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated."));

            var action = await CreateSut().ToggleLike(1);

            action.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task ToggleLike_OnValidationException_ShouldReturnBadRequest()
        {
            _mediator.Send(Arg.Any<ToggleLikeCommand>(), Arg.Any<CancellationToken>())
                .Returns<Task<Result<bool>>>(_ => throw new ValidationException(
                    new[] { new ValidationFailure("PostId", "invalid") }));

            var action = await CreateSut().ToggleLike(0);

            action.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
