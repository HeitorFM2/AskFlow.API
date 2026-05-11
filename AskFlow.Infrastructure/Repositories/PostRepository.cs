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

        public async Task<IReadOnlyList<PostsViewModel>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var data = await _context.Posts
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Content,
                    p.CreatedAt,
                    CommentsCount = p.Comments.Count(),
                    LikesCount = p.Likes.Count(),
                    p.User.UserName,
                    p.User.Identification
                })
                .ToListAsync(cancellationToken);

            return data.Select(p => new PostsViewModel
            {
                Id = p.Id,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                Comments = p.CommentsCount,
                Likes = p.LikesCount,
                User = new UserViewModel
                {
                    UserName = p.UserName ?? "",
                    Identification = p.Identification
                }
            }).ToList();
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Posts.CountAsync(cancellationToken);
        }

        public async Task<PostViewModel?> GetByIdAsync(int id)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Content,
                    p.CreatedAt,
                    LikesCount = p.Likes.Count(),
                    p.User.UserName,
                    p.User.Identification,
                    Comments = p.Comments
                        .Where(c => c.ParentCommentId == null)
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => new
                        {
                            c.Id,
                            c.Content,
                            c.CreatedAt,
                            c.ParentCommentId,
                            ReplyCount = c.Replies.Count(),
                            CommentUserName = c.User.UserName,
                            CommentIdentification = c.User.Identification
                        })
                })
                .FirstOrDefaultAsync();

            if (post is null)
                return null;

            return new PostViewModel
            {
                Id = post.Id,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                Likes = post.LikesCount,
                User = new UserViewModel
                {
                    UserName = post.UserName ?? "",
                    Identification = post.Identification
                },
                Comments = post.Comments.Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    ParentCommentId = c.ParentCommentId,
                    ReplyCount = c.ReplyCount,
                    User = new UserViewModel
                    {
                        UserName = c.CommentUserName ?? "",
                        Identification = c.CommentIdentification
                    }
                })
            };
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
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }
}
