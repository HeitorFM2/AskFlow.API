using AskFlow.Application.Auth.ViewModels;
using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Auth.Commands
{
    public record LoginCommand(string Email, string Password) : IRequest<Result<AuthViewModel>>;
}
