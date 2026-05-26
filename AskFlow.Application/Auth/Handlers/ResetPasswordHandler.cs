using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Common;
using AskFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AskFlow.Application.Auth.Handlers
{
    public class ResetPasswordHandler(
        UserManager<User> userManager,
        ILogger<ResetPasswordHandler> logger) : IRequestHandler<ResetPasswordCommand, Result>
    {
        public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                logger.LogWarning("Password reset attempted for unknown email.");
                return Result.Invalid(ErrorCodes.AuthResetTokenInvalid, "Invalid or expired reset token.");
            }

            var identityResult = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!identityResult.Succeeded)
            {
                logger.LogWarning("Password reset failed for user {UserId}.", user.Id);
                return Result.Invalid(ErrorCodes.AuthResetTokenInvalid, "Invalid or expired reset token.");
            }

            logger.LogInformation("Password reset successfully for user {UserId}.", user.Id);

            return Result.Success();
        }
    }
}
