using AskFlow.Application.Posts.Command;
using AskFlow.Application.Posts.Queries;
using AskFlow.WebAPI.Extensions;
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

        [HttpPost("ByUserName")]
        public async Task<IActionResult> GetByUserName(
            [FromBody] GetPostsByUserNameQuery query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(query with { Page = page, PageSize = pageSize });
            return result.ToActionResult(this);
        }

        [HttpGet("Following")]
        public async Task<IActionResult> GetFollowingPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetFollowingPostsQuery(page, pageSize));
            return result.ToActionResult(this);
        }

        [HttpGet("Me")]
        public async Task<IActionResult> GetMyPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetMyPostsQuery(page, pageSize));
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
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return result.ToActionResult(this);

            return CreatedAtAction(nameof(GetById), new { postId = result.Value }, new { id = result.Value });
        }

        [HttpDelete("{postId:int}")]
        public async Task<IActionResult> DeletePost([FromRoute] int postId)
        {
            var result = await _mediator.Send(new DeletePostCommand(postId));
            return result.ToActionResult(this);
        }
    }
}
