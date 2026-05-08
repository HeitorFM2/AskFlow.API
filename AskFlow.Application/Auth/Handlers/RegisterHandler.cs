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
    public class RegisterHandler(
        UserManager<User> userManager,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<RegisterHandler> logger) : IRequestHandler<RegisterCommand, Result<AuthViewModel>>
    {
        public async Task<Result<AuthViewModel>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                Identification = request.Identification,
                CreatedAt = DateTime.UtcNow
            };

            var identityResult = await userManager.CreateAsync(user, request.Password);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                logger.LogWarning("Falha ao registrar usuário {Email}: {Errors}", request.Email, errors);
                return Result<AuthViewModel>.Invalid(errors);
            }

            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken();

            await refreshTokenRepository.AddAsync(new RefreshToken(
                refreshToken,
                user,
                DateTime.UtcNow.AddDays(tokenService.RefreshTokenExpiresInDays)));

            logger.LogInformation("Usuário {Email} registrado com sucesso.", request.Email);

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
