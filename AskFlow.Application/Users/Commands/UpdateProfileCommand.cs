using AskFlow.Application.Common;
using AskFlow.Application.Users.ViewModels;
using MediatR;

namespace AskFlow.Application.Users.Commands
{
    public record UpdateProfileCommand(string UserName, string Identification) : IRequest<Result<UserDto>>;
}
