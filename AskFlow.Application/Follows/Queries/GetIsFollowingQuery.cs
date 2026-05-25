using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Follows.Queries
{
    public record GetIsFollowingQuery(string TargetUserName) : IRequest<Result<bool>>;
}
