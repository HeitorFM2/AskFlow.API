using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Likes.Commands
{
    public record ToggleLikeCommand(int PostId) : IRequest<Result<bool>>;
}
