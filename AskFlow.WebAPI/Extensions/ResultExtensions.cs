using AskFlow.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace AskFlow.WebAPI.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
        {
            return result.Type switch
            {
                ResultType.Ok => controller.Ok(result.Value),
                ResultType.NotFound => controller.NotFound(new { message = result.Error }),
                ResultType.Unauthorized => controller.Unauthorized(new { message = result.Error }),
                ResultType.Invalid => controller.BadRequest(new { message = result.Error }),
                _ => controller.StatusCode(500, new { message = result.Error })
            };
        }

        public static IActionResult ToActionResult(this Result result, ControllerBase controller)
        {
            return result.Type switch
            {
                ResultType.Ok => controller.NoContent(),
                ResultType.NotFound => controller.NotFound(new { message = result.Error }),
                ResultType.Unauthorized => controller.Unauthorized(new { message = result.Error }),
                ResultType.Invalid => controller.BadRequest(new { message = result.Error }),
                _ => controller.StatusCode(500, new { message = result.Error })
            };
        }
    }
}
