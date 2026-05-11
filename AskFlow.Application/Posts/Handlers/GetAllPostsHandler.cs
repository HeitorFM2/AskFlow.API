using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using MediatR;

namespace AskFlow.Application.Posts.Handlers
{
    public class GetAllPostsHandler(IPostRepository repository) : IRequestHandler<GetAllPostsQuery, Result<PagedResult<PostsViewModel>>>
    {
        public async Task<Result<PagedResult<PostsViewModel>>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await repository.CountAsync(cancellationToken);
            var posts = await repository.GetAllAsync(request.Page, request.PageSize, cancellationToken);

            return Result<PagedResult<PostsViewModel>>.Success(new PagedResult<PostsViewModel>
            {
                Items = posts,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
