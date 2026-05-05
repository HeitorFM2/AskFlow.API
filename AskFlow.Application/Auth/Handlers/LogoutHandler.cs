using AskFlow.Application.Auth.Commands;
using AskFlow.Domain.Interfaces;
using MediatR;

namespace AskFlow.Application.Auth.Handlers
{
    public class LogoutHandler(IRefreshTokenRepository refreshTokenRepository) : IRequestHandler<LogoutCommand>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

        public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            await _refreshTokenRepository.RevokeAllByUserIdAsync(request.UserId);
        }
    }
}