using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using MediatR;

namespace AskFlow.Application.Posts.Handlers
{
    public class GetByIdPostHandler(
        IPostQueries postQueries,
        ILikeQueries likeQueries,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetByIdPostQuery, Result<PostViewModel>>
    {
        public async Task<Result<PostViewModel>> Handle(GetByIdPostQuery request, CancellationToken cancellationToken)
        {
            var post = await postQueries.GetByIdAsync(request.postId, cancellationToken);

            if (post is null)
                return Result<PostViewModel>.NotFound(ErrorCodes.PostNotFound, "Post not found.");

            var userId = currentUserService.GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                var likedIds = await likeQueries.GetLikedPostIdsAsync(userId, [post.Id], cancellationToken);
                post.IsLiked = likedIds.Contains(post.Id);
            }

            return Result<PostViewModel>.Success(post);
        }
    }
}
