using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using MediatR;

namespace AskFlow.Application.Posts.Handlers
{
    public class GetMyPostsHandler(
        IPostQueries postQueries,
        ILikeQueries likeQueries,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetMyPostsQuery, Result<PagedResult<PostsViewModel>>>
    {
        public async Task<Result<PagedResult<PostsViewModel>>> Handle(GetMyPostsQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<PagedResult<PostsViewModel>>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var totalCount = await postQueries.CountByUserAsync(userId, cancellationToken);
            var posts = await postQueries.GetByUserAsync(userId, request.Page, request.PageSize, cancellationToken);

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
