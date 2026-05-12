using AskFlow.Application.Common;
using AskFlow.Application.Users.ViewModels;
using MediatR;

namespace AskFlow.Application.Users.Queries
{
    public record GetCurrentUserQuery() : IRequest<Result<UserDto>>;
}
