using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Domain.Interfaces;
using MediatR;

namespace AskFlow.Application.Posts.Handlers
{
    public class GetAllPostsHandler(IPostRepository repository) : IRequestHandler<GetAllPostsQuery, IEnumerable<PostsViewModel>>
    {
        private readonly IPostRepository _repository = repository;

        public async Task<IEnumerable<PostsViewModel>> Handle(
            GetAllPostsQuery request,
            CancellationToken cancellationToken)
        {
            var posts = _repository.GetAll();

            return posts.Select(p => new PostsViewModel
            {
                Id = p.Id,  
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                Comments = p.Comments.Count,
                Likes = p.Likes.Count,
                User = new UserViewModel
                {
                    UserName = p.User.UserName ?? string.Empty,
                    Identification = p.User.Identification
                }
            });
        }
    }
}