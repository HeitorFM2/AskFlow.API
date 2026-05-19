using AskFlow.Application.Common;
using AskFlow.Application.Follows.ViewModels;
using MediatR;

namespace AskFlow.Application.Follows.Queries
{
    public record GetFollowStatsQuery : IRequest<Result<FollowStatsViewModel>>;
}
