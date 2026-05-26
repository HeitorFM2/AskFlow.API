using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Auth.Commands
{
    public record ResetPasswordCommand(
        string Email,
        string Token,
        string NewPassword,
        string ConfirmPassword) : IRequest<Result>;
}
