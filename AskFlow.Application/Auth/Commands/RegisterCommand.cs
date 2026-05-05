using AskFlow.Application.Auth.ViewModels;
using MediatR;

namespace AskFlow.Application.Auth.Commands
{
    public record RegisterCommand(
        string Email,
        string Password,
        string Identification) : IRequest<AuthViewModel>;
}
