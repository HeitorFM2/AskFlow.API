using AskFlow.Application.Posts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace AskFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v2/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PostsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllPostsQuery());
            return Ok(result);
        }
    }
}