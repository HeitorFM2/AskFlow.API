using AskFlow.Application.Auth.ViewModels;
using MediatR;

namespace AskFlow.Application.Auth.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthViewModel>;
}
