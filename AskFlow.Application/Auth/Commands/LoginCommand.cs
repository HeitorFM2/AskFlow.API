using AskFlow.Application.Auth.ViewModels;
using MediatR;

namespace AskFlow.Application.Auth.Commands
{
    public record LoginCommand(string Email, string Password) : IRequest<AuthViewModel>;
}
