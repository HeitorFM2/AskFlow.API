using AskFlow.Application.Interfaces;
using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class PostQueries(AppDbContext context) : IPostQueries
    {
        private readonly AppDbContext _context = context;

        public async Task<IReadOnlyList<PostsViewModel>> GetAllAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostsViewModel
                {
                    Id = p.Id,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    Comments = p.CommentCount,
                    Likes = p.LikeCount,
                    User = new UserDto
                    {
                        UserName = p.User.UserName ?? "",
                        Identification = p.User.Identification,
                        AvatarUrl = p.User.AvatarUrl
                    }
                })
                .ToListAsync(cancellationToken);
        }

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return _context.Posts.CountAsync(cancellationToken);
        }

        public Task<PostViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Posts
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(post => new PostViewModel
                {
                    Id = post.Id,
                    Content = post.Content,
                    CreatedAt = post.CreatedAt,
                    Likes = post.LikeCount,
                    User = new UserDto
                    {
                        UserName = post.User.UserName ?? "",
                        Identification = post.User.Identification,
                        AvatarUrl = post.User.AvatarUrl
                    },
                    Comments = post.Comments
                        .Where(c => c.ParentCommentId == null)
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => new CommentViewModel
                        {
                            Id = c.Id,
                            Content = c.Content,
                            CreatedAt = c.CreatedAt,
                            ParentCommentId = c.ParentCommentId,
                            ReplyCount = c.Replies.Count,
                            User = new UserDto
                            {
                                UserName = c.User.UserName ?? "",
                                Identification = c.User.Identification,
                                AvatarUrl = c.User.AvatarUrl
                            }
                        })
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
