using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Auth.Commands
{
    public record LogoutCommand(string UserId) : IRequest<Result>;
}
