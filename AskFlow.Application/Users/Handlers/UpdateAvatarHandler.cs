using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.Commands;
using AskFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AskFlow.Application.Users.Handlers
{
    public class UpdateAvatarHandler(
        UserManager<User> userManager,
        IAvatarStorage avatarStorage,
        IImageProcessor imageProcessor,
        ICurrentUserService currentUserService,
        ILogger<UpdateAvatarHandler> logger) : IRequestHandler<UpdateAvatarCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<string>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
                return Result<string>.NotFound(ErrorCodes.UserNotFound, "User not found.");

            var processed = await imageProcessor.ProcessAvatarAsync(request.Content, cancellationToken);

            if (processed is null)
                return Result<string>.Invalid(ErrorCodes.AvatarInvalidContent, "Avatar content is not a valid image.");

            await using (processed)
            {
                await avatarStorage.DeleteAsync(userId, cancellationToken);
                var url = await avatarStorage.UploadAsync(processed.Content, processed.ContentType, userId, cancellationToken);

                user.AvatarUrl = url;
                var updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to persist avatar URL for user {UserId}: {Errors}", userId, errors);
                    return Result<string>.Failure(ErrorCodes.AvatarUploadFailed, errors);
                }

                logger.LogInformation("Avatar updated for user {UserId}.", userId);
                return Result<string>.Success(url);
            }
        }
    }
}
