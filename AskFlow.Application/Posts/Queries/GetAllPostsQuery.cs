using AskFlow.Application.Posts.ViewModels;
using MediatR;

namespace AskFlow.Application.Posts.Queries
{
    public record GetAllPostsQuery : IRequest<IEnumerable<PostsViewModel>>;
}
