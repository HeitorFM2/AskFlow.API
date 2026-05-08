using AskFlow.Application.Auth.ViewModels;
using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Auth.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthViewModel>>;
}
