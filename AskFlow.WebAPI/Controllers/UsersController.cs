using AskFlow.Application.Common;
using AskFlow.Application.Users.Commands;
using AskFlow.Application.Users.Queries;
using AskFlow.WebAPI.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskFlow.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v2/[controller]")]
    public class UsersController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {
            var result = await _mediator.Send(new GetAllUsersQuery(search));
            return result.ToActionResult(this);
        }

        [HttpGet("Me")]
        public async Task<IActionResult> GetMe()
        {
            var result = await _mediator.Send(new GetCurrentUserQuery());
            return result.ToActionResult(this);
        }

        [HttpPatch("Avatar")]
        [RequestSizeLimit(3 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 3 * 1024 * 1024)]
        public async Task<IActionResult> UpdateAvatar(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { code = ErrorCodes.AvatarRequired, message = "Avatar file is required." });

            await using var stream = file.OpenReadStream();
            var result = await _mediator.Send(new UpdateAvatarCommand(stream, file.ContentType, file.Length));

            return result.ToActionResult(this);
        }
    }
}
