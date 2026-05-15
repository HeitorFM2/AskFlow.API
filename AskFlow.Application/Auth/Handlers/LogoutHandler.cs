using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using MediatR;

namespace AskFlow.Application.Auth.Handlers
{
    public class LogoutHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand, Result>
    {
        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            await refreshTokenRepository.RevokeAllByUserIdAsync(request.UserId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
