using MediatR;

namespace AskFlow.Application.Posts.Command
    {
        public record DeletePostCommand(int PostId) : IRequest;
    }
