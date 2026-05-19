using AskFlow.Application.Follows.Commands;
using AskFlow.Application.Follows.Queries;
using AskFlow.WebAPI.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskFlow.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v2/[controller]")]
    public class FollowsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> ToggleFollow([FromBody] ToggleFollowCommand followCommand)
        {
            var result = await _mediator.Send(followCommand);
            if (!result.IsSuccess)
                return result.ToActionResult(this);
            return CreatedAtAction(nameof(ToggleFollow), new { followCommand.TargetUserName }, new { following = result.Value });
        }

        [HttpGet("Followers")]
        public async Task<IActionResult> GetFollowers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var result = await _mediator.Send(new GetFollowersQuery(page, pageSize, search));
            return result.ToActionResult(this);
        }

        [HttpGet("Following")]
        public async Task<IActionResult> GetFollowing(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var result = await _mediator.Send(new GetFollowingQuery(page, pageSize, search));
            return result.ToActionResult(this);
        }

        [HttpGet("Stats")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _mediator.Send(new GetFollowStatsQuery());
            return result.ToActionResult(this);
        }
    }
}
