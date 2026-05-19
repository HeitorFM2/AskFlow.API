using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Follows.Commands
{
    public record ToggleFollowCommand(string TargetUserName) : IRequest<Result<bool>>;
}
