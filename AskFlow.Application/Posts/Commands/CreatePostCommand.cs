using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Posts.Command
{
    public record CreatePostCommand(string Content) : IRequest<Result<int>>;
}
