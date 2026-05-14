using AskFlow.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Tests.Common.Fixtures
{
    public static class SignInManagerFixture
    {
        public static SignInManager<User> Create(UserManager<User> userManager)
        {
            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<User>>();
            return Substitute.For<SignInManager<User>>(
                userManager, contextAccessor, claimsFactory,
                null!, null!, null!, null!);
        }
    }
}
