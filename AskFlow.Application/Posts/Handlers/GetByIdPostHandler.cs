using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using MediatR;

namespace AskFlow.Application.Posts.Handlers
{
    public class GetByIdPostHandler(
        IPostRepository postRepository,
        ILikeRepository likeRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetByIdPostQuery, Result<PostViewModel>>
    {
        public async Task<Result<PostViewModel>> Handle(GetByIdPostQuery request, CancellationToken cancellationToken)
        {
            var post = await postRepository.GetByIdAsync(request.postId);

            if (post is null)
                return Result<PostViewModel>.NotFound(ErrorCodes.PostNotFound, "Post not found.");

            var userId = currentUserService.GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                var likedIds = await likeRepository.GetLikedPostIdsAsync(userId, [post.Id], cancellationToken);
                post.IsLiked = likedIds.Contains(post.Id);
            }

            return Result<PostViewModel>.Success(post);
        }
    }
}
