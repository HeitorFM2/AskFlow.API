using AskFlow.Application.Auth.ViewModels;
using MediatR;

namespace AskFlow.Application.Auth.Commands
{
    public record LogoutCommand(string UserId) : IRequest;
}
