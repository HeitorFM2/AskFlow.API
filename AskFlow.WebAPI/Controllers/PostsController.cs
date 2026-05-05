using AskFlow.Application.Posts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace AskFlow.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v2/[controller]")]
    public class PostsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllPostsQuery());
            return Ok(result);
        }
    }
}