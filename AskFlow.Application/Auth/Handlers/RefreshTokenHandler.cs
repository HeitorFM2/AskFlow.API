using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.ViewModels;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AskFlow.Application.Auth.Handlers
{
    public class RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        ILogger<RefreshTokenHandler> logger) : IRequestHandler<RefreshTokenCommand, Result<AuthViewModel>>
    {
        public async Task<Result<AuthViewModel>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

            if (refreshToken is null)
                return Result<AuthViewModel>.Unauthorized("Refresh token inválido.");

            if (!refreshToken.IsActive)
            {
                logger.LogWarning("Uso de refresh token inativo para usuário {UserId}.", refreshToken.UserId);
                return Result<AuthViewModel>.Unauthorized("Refresh token expirado ou revogado.");
            }

            refreshToken.Revoke();

            var newAccessToken = tokenService.GenerateAccessToken(refreshToken.User);
            var newRefreshToken = tokenService.GenerateRefreshToken();

            await refreshTokenRepository.AddAsync(new Domain.Entities.RefreshToken(
                newRefreshToken,
                refreshToken.User,
                DateTime.UtcNow.AddDays(tokenService.RefreshTokenExpiresInDays)));

            return Result<AuthViewModel>.Success(new AuthViewModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = tokenService.GetAccessTokenExpiry(),
                User = new UserAuthViewModel
                {
                    Id = refreshToken.User.Id,
                    Email = refreshToken.User.Email!,
                    Identification = refreshToken.User.Identification
                }
            });
        }
    }
}
