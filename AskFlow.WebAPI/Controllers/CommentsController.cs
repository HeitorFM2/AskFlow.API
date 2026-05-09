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
    [Route("api/v2")]
    public class CommentsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("posts/{postId:int}/comments")]
        public async Task<IActionResult> GetByPost(
            [FromRoute] int postId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetCommentsByPostQuery(postId, page, pageSize));
            return result.ToActionResult(this);
        }

        [HttpGet("comments/{commentId:int}/replies")]
        public async Task<IActionResult> GetReplies(
            [FromRoute] int commentId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetCommentRepliesQuery(commentId, page, pageSize));
            return result.ToActionResult(this);
        }

        [HttpPost("posts/{postId:int}/comments")]
        public async Task<IActionResult> CreateComment(
            [FromRoute] int postId,
            [FromBody] CreateCommentBody body)
        {
            try
            {
                var command = new CreateCommentCommand(postId, body.Content, body.ParentCommentId);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                    return result.ToActionResult(this);

                return CreatedAtAction(nameof(GetByPost), new { postId }, new { id = result.Value });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)) });
            }
        }

        [HttpDelete("comments/{commentId:int}")]
        public async Task<IActionResult> DeleteComment([FromRoute] int commentId)
        {
            try
            {
                var result = await _mediator.Send(new DeleteCommentCommand(commentId));
                return result.ToActionResult(this);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)) });
            }
        }
    }

    public record CreateCommentBody(string Content, int? ParentCommentId = null);
}
