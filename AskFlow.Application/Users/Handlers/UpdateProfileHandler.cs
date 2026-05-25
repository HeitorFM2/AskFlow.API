using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.Commands;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Application.Users.Handlers
{
    public class UpdateProfileHandler(
        UserManager<User> userManager,
        IUserQueries userQueries,
        ICurrentUserService currentUserService) : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<UserDto>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
                return Result<UserDto>.NotFound(ErrorCodes.UserNotFound, "User not found.");

            var userNameOwner = await userManager.FindByNameAsync(request.UserName);
            if (userNameOwner is not null && userNameOwner.Id != userId)
                return Result<UserDto>.Conflict(ErrorCodes.UserNameTaken, "UserName is already taken.");

            if (await userQueries.IsIdentificationTakenAsync(request.Identification, userId, cancellationToken))
                return Result<UserDto>.Conflict(ErrorCodes.IdentificationTaken, "Identification is already taken.");

            user.UserName = request.UserName;
            user.NormalizedUserName = userManager.NormalizeName(request.UserName);
            user.Identification = request.Identification;

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<UserDto>.Failure(ErrorCodes.AuthIdentityFailure, errors);
            }

            return Result<UserDto>.Success(new UserDto
            {
                UserName = user.UserName,
                Identification = user.Identification,
                AvatarUrl = user.AvatarUrl
            });
        }
    }
}
