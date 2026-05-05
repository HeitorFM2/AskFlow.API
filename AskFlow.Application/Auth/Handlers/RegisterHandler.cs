using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.Settings;
using AskFlow.Application.Auth.ViewModels;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AskFlow.Application.Auth.Handlers
{
    public class RegisterHandler(
        UserManager<User> userManager,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtSettings> jwtSettings) : IRequestHandler<RegisterCommand, AuthViewModel>
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        public async Task<AuthViewModel> Handle(
            RegisterCommand request,
            CancellationToken cancellationToken)
        {
            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                Identification = request.Identification,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(new RefreshToken(
                refreshToken,
                user,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays)));

            return new AuthViewModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
                User = new UserAuthViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    Identification = user.Identification
                }
            };
        }
    }
}