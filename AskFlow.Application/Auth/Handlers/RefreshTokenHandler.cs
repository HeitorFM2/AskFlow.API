using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.Settings;
using AskFlow.Application.Auth.ViewModels;
using AskFlow.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace AskFlow.Application.Auth.Handlers
{
    public class RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings) : IRequestHandler<RefreshTokenCommand, AuthViewModel>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
        private readonly ITokenService _tokenService = tokenService;
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        public async Task<AuthViewModel> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken)
                ?? throw new Exception("Refresh token inválido.");

            if (!refreshToken.IsActive)
                throw new Exception("Refresh token expirado ou revogado.");

            refreshToken.Revoke();

            var newAccessToken = _tokenService.GenerateAccessToken(refreshToken.User);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(new Domain.Entities.RefreshToken(
                newRefreshToken,
                refreshToken.User,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays)));

            return new AuthViewModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
                User = new UserAuthViewModel
                {
                    Id = refreshToken.User.Id,
                    Email = refreshToken.User.Email!,
                    Identification = refreshToken.User.Identification
                }
            };
        }
    }
}