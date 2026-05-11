using AskFlow.Application.Comments.Commands;
using AskFlow.Application.Comments.Queries;
using AskFlow.WebAPI.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskFlow.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v2/[controller]")]
    public class CommentsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("{postId:int}")]
        public async Task<IActionResult> GetByPost(
            [FromRoute] int postId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetCommentsByPostQuery(postId, page, pageSize));
            return result.ToActionResult(this);
        }

        [HttpGet("{commentId:int}/Replies")]
        public async Task<IActionResult> GetReplies(
            [FromRoute] int commentId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetCommentRepliesQuery(commentId, page, pageSize));
            return result.ToActionResult(this);
        }

        [HttpPost("{postId:int}")]
        public async Task<IActionResult> CreateComment(
            [FromRoute] int postId,
            [FromBody] CreateCommentCommand command)
        {
            try
            {
                var result = await _mediator.Send(command with { PostId = postId });

                if (!result.IsSuccess)
                    return result.ToActionResult(this);

                return CreatedAtAction(nameof(GetByPost), new { postId }, new { id = result.Value });
            }
            catch (ValidationException ex)
            {
                return ex.ToValidationActionResult(this);
            }
        }

        [HttpDelete("{commentId:int}")]
        public async Task<IActionResult> DeleteComment([FromRoute] int commentId)
        {
            try
            {
                var result = await _mediator.Send(new DeleteCommentCommand(commentId));
                return result.ToActionResult(this);
            }
            catch (ValidationException ex)
            {
                return ex.ToValidationActionResult(this);
            }
        }
    }

}
