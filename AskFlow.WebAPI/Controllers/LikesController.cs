using AskFlow.Application.Likes.Commands;
using AskFlow.Application.Likes.Queries;
using AskFlow.WebAPI.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskFlow.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v2/[controller]")]
    public class LikesController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("LikedPosts")]
        public async Task<IActionResult> GetLikedPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetLikedPostsQuery(page, pageSize));
            return result.ToActionResult(this);
        }

        [HttpPost("{postId:int}")]
        public async Task<IActionResult> ToggleLike([FromRoute] int postId)
        {
            var result = await _mediator.Send(new ToggleLikeCommand(postId));

            if (!result.IsSuccess)
                return result.ToActionResult(this);

            return CreatedAtAction(nameof(ToggleLike), new { postId }, new { liked = result.Value });
        }
    }
}
