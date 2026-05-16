using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Likes.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using MediatR;

namespace AskFlow.Application.Likes.Handlers
{
    public class GetLikedPostsHandler(
        ILikeQueries queries,
        ICurrentUserService currentUserService) : IRequestHandler<GetLikedPostsQuery, Result<PagedResult<PostsViewModel>>>
    {
        public async Task<Result<PagedResult<PostsViewModel>>> Handle(GetLikedPostsQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<PagedResult<PostsViewModel>>.Unauthorized(
                    ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var totalCount = await queries.CountLikedPostAsync(userId, cancellationToken);
            var likedPosts = await queries.GetLikePostsAsync(userId, request.Page, request.PageSize, cancellationToken);

            var items = likedPosts.Select(p => new PostsViewModel
            {
                Id = p.PostId,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                Comments = p.CommentsCount,
                Likes = p.LikesCount,
                IsLiked = true,
                User = new UserDto
                {
                    UserName = p.AuthorUserName,
                    Identification = p.AuthorIdentification,
                    AvatarUrl = p.AuthorAvatarUrl
                }
            }).ToList();

            return Result<PagedResult<PostsViewModel>>.Success(new PagedResult<PostsViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
