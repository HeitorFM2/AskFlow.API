using AskFlow.Application.Comments.Commands;
using AskFlow.Application.Comments.Queries;
using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Common;
using AskFlow.WebAPI.Controllers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AskFlow.Tests.WebAPI.Controllers
{
    public class CommentsControllerTests
    {
        private readonly IMediator _mediator = Substitute.For<IMediator>();
        private CommentsController CreateSut() => new(_mediator);

        [Fact]
        public async Task GetByPost_ShouldReturnOk()
        {
            _mediator.Send(Arg.Any<GetCommentsByPostQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<CommentViewModel>>.Success(new PagedResult<CommentViewModel>()));

            var action = await CreateSut().GetByPost(1);

            action.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetReplies_ShouldReturnOk()
        {
            _mediator.Send(Arg.Any<GetCommentRepliesQuery>(), Arg.Any<CancellationToken>())
                .Returns(Result<PagedResult<CommentViewModel>>.Success(new PagedResult<CommentViewModel>()));

            var action = await CreateSut().GetReplies(1);

            action.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateComment_OnSuccess_ShouldReturnCreatedAtAction()
        {
            _mediator.Send(Arg.Any<CreateCommentCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<int>.Success(99));

            var action = await CreateSut().CreateComment(1, new CreateCommentBody("oi"));

            var created = action.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.ActionName.Should().Be(nameof(CommentsController.GetByPost));
            created.RouteValues!["postId"].Should().Be(1);
        }

        [Fact]
        public async Task CreateComment_OnFailure_ShouldReturnMappedActionResult()
        {
            _mediator.Send(Arg.Any<CreateCommentCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result<int>.NotFound("post não existe"));

            var action = await CreateSut().CreateComment(1, new CreateCommentBody("oi"));

            action.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateComment_OnValidationException_ShouldReturnBadRequest()
        {
            _mediator.Send(Arg.Any<CreateCommentCommand>(), Arg.Any<CancellationToken>())
                .Returns<Task<Result<int>>>(_ => throw new ValidationException(new[] { new ValidationFailure("Content", "obrig") }));

            var action = await CreateSut().CreateComment(1, new CreateCommentBody(""));

            action.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteComment_OnSuccess_ShouldReturnNoContent()
        {
            _mediator.Send(Arg.Any<DeleteCommentCommand>(), Arg.Any<CancellationToken>())
                .Returns(Result.Success());

            var action = await CreateSut().DeleteComment(1);

            action.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteComment_OnValidationException_ShouldReturnBadRequest()
        {
            _mediator.Send(Arg.Any<DeleteCommentCommand>(), Arg.Any<CancellationToken>())
                .Returns<Task<Result>>(_ => throw new ValidationException(new[] { new ValidationFailure("CommentId", "x") }));

            var action = await CreateSut().DeleteComment(0);

            action.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
