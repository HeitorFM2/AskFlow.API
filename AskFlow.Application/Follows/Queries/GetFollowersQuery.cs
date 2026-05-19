using AskFlow.Application.Common;
using AskFlow.Application.Follows.ViewModels;
using MediatR;

namespace AskFlow.Application.Follows.Queries
{
    public record GetFollowersQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<Result<PagedResult<FollowViewModel>>>;
}
