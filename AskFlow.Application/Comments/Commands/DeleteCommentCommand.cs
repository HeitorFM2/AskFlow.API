using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Comments.Commands
{
    public record DeleteCommentCommand(int CommentId) : IRequest<Result>;
}
