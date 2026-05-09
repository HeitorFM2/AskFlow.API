using AskFlow.Application.Posts.Command;
using AskFlow.Application.Posts.Queries;
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
    public class PostsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetAllPostsQuery(page, pageSize));
            return result.ToActionResult(this);
        }

        [HttpGet("{postId:int}/Details")]
        public async Task<IActionResult> GetById([FromRoute] int postId)
        {
            var result = await _mediator.Send(new GetByIdPostQuery(postId));
            return result.ToActionResult(this);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                    return result.ToActionResult(this);

                return CreatedAtAction(nameof(GetById), new { postId = result.Value }, new { id = result.Value });
            }
            catch (ValidationException ex)
            {
                return ex.ToValidationActionResult(this);
            }
        }

        [HttpDelete("{postId:int}")]
        public async Task<IActionResult> DeletePost([FromRoute] int postId)
        {
            try
            {
                var result = await _mediator.Send(new DeletePostCommand(postId));
                return result.ToActionResult(this);
            }
            catch (ValidationException ex)
            {
                return ex.ToValidationActionResult(this);
            }
        }
    }
}
