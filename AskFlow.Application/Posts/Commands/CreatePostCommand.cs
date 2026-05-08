using MediatR;

namespace AskFlow.Application.Posts.Command
    {
        public record CreatePostCommand(string Content) : IRequest<int>;
    }
