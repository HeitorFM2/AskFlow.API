using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.ViewModels;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AskFlow.Application.Auth.Handlers
{
    public class LoginHandler(
        UserManager<User> userManager,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<LoginHandler> logger) : IRequestHandler<LoginCommand, Result<AuthViewModel>>
    {
        public async Task<Result<AuthViewModel>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                logger.LogWarning("Tentativa de login inválida para {Email}.", request.Email);
                return Result<AuthViewModel>.Unauthorized("Email ou senha inválidos.");
            }

            await refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);

            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken();

            await refreshTokenRepository.AddAsync(new RefreshToken(
                refreshToken,
                user,
                DateTime.UtcNow.AddDays(tokenService.RefreshTokenExpiresInDays)));

            logger.LogInformation("Usuário {Email} autenticado com sucesso.", request.Email);

            return Result<AuthViewModel>.Success(new AuthViewModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = tokenService.GetAccessTokenExpiry(),
                User = new UserAuthViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    Identification = user.Identification
                }
            });
        }
    }
}
