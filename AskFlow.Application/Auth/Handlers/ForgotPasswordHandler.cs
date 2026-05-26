using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AskFlow.Application.Auth.Handlers
{
    public class ForgotPasswordHandler(
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<ForgotPasswordHandler> logger) : IRequestHandler<ForgotPasswordCommand, Result>
    {
        public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                logger.LogInformation("Password reset requested for unknown email.");
                return Result.Success();
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            await emailService.SendPasswordResetEmailAsync(request.Email, token, cancellationToken);

            logger.LogInformation("Password reset email sent for user {UserId}.", user.Id);

            return Result.Success();
        }
    }
}
