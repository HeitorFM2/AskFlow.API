using AskFlow.Application.Interfaces;
using AskFlow.Infrastructure.Data;
using AskFlow.Infrastructure.Data.Interceptors;
using AskFlow.Infrastructure.Repositories;
using AskFlow.Infrastructure.Services;
using AskFlow.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AskFlow.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<AvatarCleanupInterceptor>();

            services.AddDbContext<AppDbContext>((sp, options) =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null))
                .AddInterceptors(sp.GetRequiredService<AvatarCleanupInterceptor>())); 

            services.Configure<JwtSettings>(
                configuration.GetSection("JwtSettings"));

            services.Configure<BlobStorageSettings>(
                configuration.GetSection("BlobStorage"));

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IPasswordSignInService, IdentityPasswordSignInService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAvatarStorage, AzureBlobAvatarStorage>();
            services.AddSingleton<IImageProcessor, ImageSharpAvatarProcessor>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();

            services.AddHostedService<RefreshTokenCleanupService>();

            return services;
        }
    }
}
