using AskFlow.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace AskFlow.WebAPI.Extensions
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ValidationException validation)
            {
                var errors = validation.Errors.Select(e => new
                {
                    code = e.ErrorCode,
                    field = e.PropertyName,
                    message = e.ErrorMessage
                });

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    code = ErrorCodes.ValidationError,
                    message = "One or more validation errors occurred.",
                    errors
                }, cancellationToken);
                return true;
            }

            logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                code = ErrorCodes.InternalServerError,
                message = "An unexpected error occurred.",
                traceId = context.TraceIdentifier
            }, cancellationToken);
            return true;
        }
    }
}
