using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Common;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using MediatR;

namespace AskFlow.Application.Posts.Handlers
{
    public class GetByIdPostHandler(IPostRepository repository) : IRequestHandler<GetByIdPostQuery, Result<PostViewModel>>
    {
        public async Task<Result<PostViewModel>> Handle(GetByIdPostQuery request, CancellationToken cancellationToken)
        {
            var post = await repository.GetByIdAsync(request.postId);

            if (post is null)
                return Result<PostViewModel>.NotFound("Post não encontrado.");

            return Result<PostViewModel>.Success(MapToViewModel(post));
        }

        private static PostViewModel MapToViewModel(Post post) => new()
        {
            Id = post.Id,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            Likes = post.Likes.Count,
            Comments = post.Comments.Select(MapToCommentViewModel),
            User = MapToUserViewModel(post.User)
        };

        private static CommentViewModel MapToCommentViewModel(Comment comment) => new()
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            User = MapToUserViewModel(comment.User)
        };

        private static UserViewModel MapToUserViewModel(User user) => new()
        {
            UserName = user.UserName ?? string.Empty,
            Identification = user.Identification
        };
    }
}
