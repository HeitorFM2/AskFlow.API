using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class UserQueries(AppDbContext context) : IUserQueries
    {
        private readonly AppDbContext _context = context;

        public async Task<IReadOnlyList<UserDto>> GetAllAsync(
            string excludeUserId,
            string? search = null,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id != excludeUserId
                    && (search == null || u.UserName!.Contains(search) || u.Identification.Contains(search)))
                .Select(u => new UserDto
                {
                    UserName = u.UserName ?? string.Empty,
                    Identification = u.Identification,
                    AvatarUrl = u.AvatarUrl
                })
                .ToListAsync(cancellationToken);
        }
    }
}
