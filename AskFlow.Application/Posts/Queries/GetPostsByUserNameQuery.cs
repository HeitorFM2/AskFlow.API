using AskFlow.Application.Common;
using AskFlow.Application.Posts.ViewModels;
using MediatR;

namespace AskFlow.Application.Posts.Queries
{
    public record GetPostsByUserNameQuery(string TargetUserName, int Page = 1, int PageSize = 20)
        : IRequest<Result<PagedResult<PostsViewModel>>>;
}
