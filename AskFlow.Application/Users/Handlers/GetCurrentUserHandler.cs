using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.Queries;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Application.Users.Handlers
{
    public class GetCurrentUserHandler(
        UserManager<User> userManager,
        ICurrentUserService currentUserService) : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<UserDto>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
                return Result<UserDto>.NotFound(ErrorCodes.UserNotFound, "User not found.");

            return Result<UserDto>.Success(new UserDto
            {
                UserName = user.UserName ?? "",
                Identification = user.Identification,
                AvatarUrl = user.AvatarUrl
            });
        }
    }
}
