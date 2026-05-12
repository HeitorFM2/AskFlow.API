using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Users.Commands
{
    public record UpdateAvatarCommand(Stream Content, string ContentType, long Length) : IRequest<Result<string>>;
}
