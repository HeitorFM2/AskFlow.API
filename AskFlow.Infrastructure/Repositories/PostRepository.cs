using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class PostRepository(AppDbContext context) : IPostRepository
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
                    Comments = p.Comments.Count,
                    Likes = p.Likes.Count,
                    User = new UserDto
                    {
                        UserName = p.User.UserName ?? "",
                        Identification = p.User.Identification

                    }
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Posts.CountAsync(cancellationToken);
        }

        public async Task<PostViewModel?> GetByIdAsync(int id)
        {
            return await _context.Posts
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(post => new PostViewModel
                {
                    Id = post.Id,
                    Content = post.Content,
                    CreatedAt = post.CreatedAt,
                    Likes = post.Likes.Count,
                    User = new UserDto
                    {
                        UserName = post.User.UserName ?? "",
                        Identification = post.User.Identification
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
                                Identification = c.User.Identification
                            }
                        })
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Post?> FindByIdAsync(int id)
        {
            return await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Post post)
        {
            post.MarkAsDeleted();
            await _context.SaveChangesAsync();
        }
    }
}
