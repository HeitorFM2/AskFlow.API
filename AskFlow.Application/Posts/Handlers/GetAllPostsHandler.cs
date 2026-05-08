using AskFlow.Application.Common;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Domain.Interfaces;
using MediatR;

namespace AskFlow.Application.Posts.Handlers
{
    public class GetAllPostsHandler(IPostRepository repository) : IRequestHandler<GetAllPostsQuery, Result<PagedResult<PostsViewModel>>>
    {
        public async Task<Result<PagedResult<PostsViewModel>>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await repository.CountAsync(cancellationToken);
            var posts = await repository.GetAllAsync(request.Page, request.PageSize, cancellationToken);

            var items = posts.Select(p => new PostsViewModel
            {
                Id = p.Id,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                Comments = p.Comments.Count,
                Likes = p.Likes.Count,
                User = new UserViewModel
                {
                    UserName = p.User.UserName ?? string.Empty,
                    Identification = p.User.Identification
                }
            }).ToList();

            return Result<PagedResult<PostsViewModel>>.Success(new PagedResult<PostsViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
