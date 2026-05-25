using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Application.Posts.Handlers
{
    public class GetPostsByUserNameHandler(
        IPostQueries postQueries,
        ILikeQueries likeQueries,
        ICurrentUserService currentUserService,
        UserManager<User> userManager)
        : IRequestHandler<GetPostsByUserNameQuery, Result<PagedResult<PostsViewModel>>>
    {
        public async Task<Result<PagedResult<PostsViewModel>>> Handle(GetPostsByUserNameQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<PagedResult<PostsViewModel>>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var targetUser = await userManager.FindByNameAsync(request.TargetUserName);

            if (targetUser is null)
                return Result<PagedResult<PostsViewModel>>.NotFound(ErrorCodes.UserNotFound, "User not found.");

            var totalCount = await postQueries.CountByUserAsync(targetUser.Id, cancellationToken);
            var posts = await postQueries.GetByUserAsync(targetUser.Id, request.Page, request.PageSize, cancellationToken);

            if (posts.Count > 0)
            {
                var likedIds = await likeQueries.GetLikedPostIdsAsync(userId, posts.Select(p => p.Id), cancellationToken);

                foreach (var post in posts)
                    post.IsLiked = likedIds.Contains(post.Id);
            }

            return Result<PagedResult<PostsViewModel>>.Success(new PagedResult<PostsViewModel>
            {
                Items = posts,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
