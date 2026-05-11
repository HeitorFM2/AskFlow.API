using AskFlow.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AskFlow.Infrastructure.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        public string? GetUserId() =>
            httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
            ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
