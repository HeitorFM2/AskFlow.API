using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.ViewModels;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
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
                logger.LogWarning("Failed to register user {Email}: {Errors}", request.Email, errors);
                return Result<AuthViewModel>.Invalid(ErrorCodes.AuthIdentityFailure, errors);
            }

            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken();

            await refreshTokenRepository.AddAsync(new RefreshToken(
                refreshToken,
                user,
                DateTime.UtcNow.AddDays(tokenService.RefreshTokenExpiresInDays)));

            logger.LogInformation("User {Email} registered successfully.", request.Email);

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
