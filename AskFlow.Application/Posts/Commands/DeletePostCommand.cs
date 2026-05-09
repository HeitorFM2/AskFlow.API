using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Posts.Command
{
    public record DeletePostCommand(int PostId) : IRequest<Result>;
}
