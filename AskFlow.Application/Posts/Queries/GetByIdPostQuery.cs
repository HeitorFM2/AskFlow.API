using AskFlow.Application.Posts.ViewModels;
using MediatR;

namespace AskFlow.Application.Posts.Queries
{
    public record GetByIdPostQuery(int postId) : IRequest<PostViewModel>;
}
