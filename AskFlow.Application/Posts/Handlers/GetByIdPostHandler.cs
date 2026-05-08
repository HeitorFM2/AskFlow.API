using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using MediatR;

namespace AskFlow.Application.Posts.Handlers
{
    public class GetByIdPostHandler(IPostRepository repository) : IRequestHandler<GetByIdPostQuery, PostViewModel>
    {
        private readonly IPostRepository _repository = repository;

        public async Task<PostViewModel> Handle(
            GetByIdPostQuery request,
            CancellationToken cancellationToken)
        {
            var post = await _repository.GetByIdAsync(request.postId)
                ?? throw new KeyNotFoundException("Post não encontrado.");

            return MapToViewModel(post);
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