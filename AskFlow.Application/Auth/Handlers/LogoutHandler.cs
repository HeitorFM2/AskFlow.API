using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Common;
using AskFlow.Domain.Interfaces;
using MediatR;

namespace AskFlow.Application.Auth.Handlers
{
    public class LogoutHandler(IRefreshTokenRepository refreshTokenRepository) : IRequestHandler<LogoutCommand, Result>
    {
        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            await refreshTokenRepository.RevokeAllByUserIdAsync(request.UserId);
            return Result.Success();
        }
    }
}
