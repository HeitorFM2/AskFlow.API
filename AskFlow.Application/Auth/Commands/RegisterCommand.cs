using AskFlow.Application.Auth.ViewModels;
using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Auth.Commands
{
    public record RegisterCommand(
        string Email,
        string UserName,
        string Password,
        string Identification) : IRequest<Result<AuthViewModel>>;
}
