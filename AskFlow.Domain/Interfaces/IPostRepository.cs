using AskFlow.Domain.Entities;

namespace AskFlow.Domain.Interfaces
{
    public interface IPostRepository
    {
        IQueryable<Post> GetAll();
    }
}
