using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Comments.Commands
{
    public record CreateCommentCommand(int PostId, string Content, int? ParentCommentId = null) : IRequest<Result<int>>;
}
