using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using MediatR;

namespace AskFlow.Application.Posts.Handlers
{
    public class GetAllPostsHandler(
        IPostQueries postQueries,
        ILikeQueries likeQueries,
        IFollowQueries followQueries,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetAllPostsQuery, Result<PagedResult<PostsViewModel>>>
    {
        public async Task<Result<PagedResult<PostsViewModel>>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await postQueries.CountAsync(cancellationToken);
            var posts = await postQueries.GetAllAsync(request.Page, request.PageSize, cancellationToken);

            var userId = currentUserService.GetUserId();
            if (!string.IsNullOrEmpty(userId) && posts.Count > 0)
            {
                var authorNames = posts.Select(p => p.User.UserName);

                var likedIds = await likeQueries.GetLikedPostIdsAsync(userId, posts.Select(p => p.Id), cancellationToken);
                var followedNames = await followQueries.GetFollowedUserNamesAsync(userId, authorNames, cancellationToken);

                foreach (var post in posts)
                {
                    post.IsLiked = likedIds.Contains(post.Id);
                    post.User.IsFollowing = followedNames.Contains(post.User.UserName);
                }
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
