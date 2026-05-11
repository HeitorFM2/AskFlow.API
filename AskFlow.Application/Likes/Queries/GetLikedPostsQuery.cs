using AskFlow.Application.Common;
using AskFlow.Application.Posts.ViewModels;
using MediatR;

namespace AskFlow.Application.Likes.Queries
{
    public record GetLikedPostsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PagedResult<PostsViewModel>>>;
}
