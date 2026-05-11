using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using MediatR;

namespace AskFlow.Application.Posts.Handlers
{
    public class GetByIdPostHandler(IPostRepository repository) : IRequestHandler<GetByIdPostQuery, Result<PostViewModel>>
    {
        public async Task<Result<PostViewModel>> Handle(GetByIdPostQuery request, CancellationToken cancellationToken)
        {
            var post = await repository.GetByIdAsync(request.postId);

            if (post is null)
                return Result<PostViewModel>.NotFound(ErrorCodes.PostNotFound, "Post not found.");

            return Result<PostViewModel>.Success(post);
        }
    }
}
