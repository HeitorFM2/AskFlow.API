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
    public class LoginHandler(
        UserManager<User> userManager,
        IPasswordSignInService passwordSignInService,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<LoginHandler> logger) : IRequestHandler<LoginCommand, Result<AuthViewModel>>
    {
        public async Task<Result<AuthViewModel>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                logger.LogWarning("Invalid login attempt for unknown user.");
                return Result<AuthViewModel>.Unauthorized(ErrorCodes.AuthInvalidCredentials, "Invalid email or password.");
            }

            var signInResult = await passwordSignInService.CheckPasswordAsync(user, request.Password, cancellationToken);

            if (signInResult == PasswordSignInResult.LockedOut)
            {
                logger.LogWarning("Account locked out for user {UserId}.", user.Id);
                return Result<AuthViewModel>.Unauthorized(ErrorCodes.AuthAccountLocked, "Account is temporarily locked due to failed login attempts.");
            }

            if (signInResult != PasswordSignInResult.Success)
            {
                logger.LogWarning("Invalid login attempt for user {UserId}.", user.Id);
                return Result<AuthViewModel>.Unauthorized(ErrorCodes.AuthInvalidCredentials, "Invalid email or password.");
            }

            await refreshTokenRepository.RevokeAllByUserIdAsync(user.Id, cancellationToken);

            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken();

            refreshTokenRepository.Add(new RefreshToken(
                refreshToken,
                user,
                DateTime.UtcNow.AddDays(tokenService.RefreshTokenExpiresInDays)));

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("User {UserId} authenticated successfully.", user.Id);

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
