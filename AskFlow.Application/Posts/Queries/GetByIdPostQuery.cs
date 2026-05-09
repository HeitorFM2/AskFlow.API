using AskFlow.Application.Common;
using AskFlow.Application.Posts.ViewModels;
using MediatR;

namespace AskFlow.Application.Posts.Queries
{
    public record GetByIdPostQuery(int postId) : IRequest<Result<PostViewModel>>;
}
