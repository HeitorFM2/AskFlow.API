using AskFlow.Application.Common;
using AskFlow.Application.Follows.Commands;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Application.Follows.Handlers
{
    public class ToggleFollowHandler(
        IFollowRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        UserManager<User> userManager) : IRequestHandler<ToggleFollowCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ToggleFollowCommand command, CancellationToken cancellationToken)
        {
            var followerId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(followerId))
                return Result<bool>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var target = await userManager.FindByNameAsync(command.TargetUserName);

            if (target is null)
                return Result<bool>.NotFound(ErrorCodes.UserNotFound, "User not found.");

            if (followerId == target.Id)
                return Result<bool>.Invalid(ErrorCodes.FollowCannotFollowSelf, "Cannot follow yourself.");

            var following = await repository.ToggleAsync(followerId, target.Id, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(following);
        }
    }
}
