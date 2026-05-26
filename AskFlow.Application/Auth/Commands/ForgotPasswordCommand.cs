using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Auth.Commands
{
    public record ForgotPasswordCommand(string Email) : IRequest<Result>;
}
