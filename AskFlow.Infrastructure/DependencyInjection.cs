using AskFlow.Application.Interfaces;
using AskFlow.Domain.Interfaces;
using AskFlow.Infrastructure.Data;
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
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            services.Configure<JwtSettings>(
                configuration.GetSection("JwtSettings"));

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            return services;
        }
    }
}
