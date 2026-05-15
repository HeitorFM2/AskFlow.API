using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.ViewModels;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AskFlow.Application.Auth.Handlers
{
    public class RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenHandler> logger) : IRequestHandler<RefreshTokenCommand, Result<AuthViewModel>>
    {
        public async Task<Result<AuthViewModel>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

            if (refreshToken is null)
                return Result<AuthViewModel>.Unauthorized(ErrorCodes.AuthRefreshTokenInvalid, "Invalid refresh token.");

            if (refreshToken.IsRevoked)
            {
                logger.LogWarning("Refresh token reuse detected for user {UserId}. Revoking all active tokens.", refreshToken.UserId);
                await refreshTokenRepository.RevokeAllByUserIdAsync(refreshToken.UserId, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<AuthViewModel>.Unauthorized(ErrorCodes.AuthRefreshTokenReuseDetected, "Refresh token reuse detected.");
            }

            if (refreshToken.IsExpired)
            {
                logger.LogWarning("Expired refresh token used by user {UserId}.", refreshToken.UserId);
                return Result<AuthViewModel>.Unauthorized(ErrorCodes.AuthRefreshTokenExpiredOrRevoked, "Refresh token expired.");
            }

            refreshToken.Revoke();

            var newAccessToken = tokenService.GenerateAccessToken(refreshToken.User);
            var newRefreshToken = tokenService.GenerateRefreshToken();

            refreshTokenRepository.Add(new Domain.Entities.RefreshToken(
                newRefreshToken,
                refreshToken.User,
                DateTime.UtcNow.AddDays(tokenService.RefreshTokenExpiresInDays)));

            await unitOfWork.SaveChangesAsync(cancellationToken);

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
