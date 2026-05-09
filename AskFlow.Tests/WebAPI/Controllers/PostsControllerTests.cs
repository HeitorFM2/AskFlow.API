using AskFlow.Application.Common;
using AskFlow.Application.Posts.Command;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.WebAPI.Controllers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AskFlow.Tests.WebAPI.Controllers
{
    public class PostsControllerTests
    {
        private readonly IMediator _mediator = Substitute.For<IMediator>();
        private PostsController CreateSut() => new(_mediator);

        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            _mediator.Send(Arg.Any<GetAllPostsQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<PostsViewModel>>.Success(new PagedResult<PostsViewModel>()));

            var action = await CreateSut().GetAll();

            action.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ShouldReturnOk()
        {
            _mediator.Send(Arg.Any<GetByIdPostQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PostViewModel>.Success(new PostViewModel
                {
                    Comments = Array.Empty<AskFlow.Application.Comments.ViewModels.CommentViewModel>(),
                    User = new UserViewModel()
                }));

            var action = await CreateSut().GetById(1);

            action.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_NotFound_ShouldReturnNotFound()
        {
            _mediator.Send(Arg.Any<GetByIdPostQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PostViewModel>.NotFound("não achou"));

            var action = await CreateSut().GetById(1);

            action.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreatePost_OnSuccess_ShouldReturnCreatedAtAction()
        {
            _mediator.Send(Arg.Any<CreatePostCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<int>.Success(7));

            var action = await CreateSut().CreatePost(new CreatePostCommand("texto"));

            var created = action.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.ActionName.Should().Be(nameof(PostsController.GetById));
            created.RouteValues!["postId"].Should().Be(7);
        }

        [Fact]
        public async Task CreatePost_OnFailure_ShouldReturnMappedActionResult()
        {
            _mediator.Send(Arg.Any<CreatePostCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<int>.Unauthorized("nope"));

            var action = await CreateSut().CreatePost(new CreatePostCommand("texto"));

            action.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task CreatePost_OnValidationException_ShouldReturnBadRequest()
        {
            _mediator.Send(Arg.Any<CreatePostCommand>(), Arg.Any<CancellationToken>())
                .Returns<Task<Result<int>>>(_ => throw new ValidationException(new[] { new ValidationFailure("Content", "obrig") }));

            var action = await CreateSut().CreatePost(new CreatePostCommand(""));

            action.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeletePost_OnSuccess_ShouldReturnNoContent()
        {
            _mediator.Send(Arg.Any<DeletePostCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result.Success());

            var action = await CreateSut().DeletePost(1);

            action.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeletePost_OnValidationException_ShouldReturnBadRequest()
        {
            _mediator.Send(Arg.Any<DeletePostCommand>(), Arg.Any<CancellationToken>())
                .Returns<Task<Result>>(_ => throw new ValidationException(new[] { new ValidationFailure("PostId", "x") }));

            var action = await CreateSut().DeletePost(0);

            action.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
