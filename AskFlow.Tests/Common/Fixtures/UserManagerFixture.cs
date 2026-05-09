using AskFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Tests.Common.Fixtures
{
    public static class UserManagerFixture
    {
        public static UserManager<User> Create()
        {
            var store = Substitute.For<IUserStore<User>>();
            return Substitute.For<UserManager<User>>(
                store, null!, null!, null!, null!, null!, null!, null!, null!);
        }
    }
}
