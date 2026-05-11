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
                ResultType.NotFound => controller.NotFound(BuildErrorBody(result)),
                ResultType.Unauthorized => controller.Unauthorized(BuildErrorBody(result)),
                ResultType.Invalid => controller.BadRequest(BuildErrorBody(result)),
                _ => controller.StatusCode(500, BuildErrorBody(result))
            };
        }

        public static IActionResult ToActionResult(this Result result, ControllerBase controller)
        {
            return result.Type switch
            {
                ResultType.Ok => controller.NoContent(),
                ResultType.NotFound => controller.NotFound(BuildErrorBody(result)),
                ResultType.Unauthorized => controller.Unauthorized(BuildErrorBody(result)),
                ResultType.Invalid => controller.BadRequest(BuildErrorBody(result)),
                _ => controller.StatusCode(500, BuildErrorBody(result))
            };
        }

        private static object BuildErrorBody(Result result) => new
        {
            code = result.ErrorCode,
            message = result.Error
        };
    }
}
