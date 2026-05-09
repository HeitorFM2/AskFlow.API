using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AskFlow.Tests.Common.Fixtures
{
    public static class HttpContextFixture
    {
        public static IHttpContextAccessor CreateAuthenticated(string userId, string claimType = ClaimTypes.NameIdentifier)
        {
            var accessor = Substitute.For<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(claimType, userId)
            }, "TestAuth");
            context.User = new ClaimsPrincipal(identity);
            accessor.HttpContext.Returns(context);
            return accessor;
        }

        public static IHttpContextAccessor CreateUnauthenticated()
        {
            var accessor = Substitute.For<IHttpContextAccessor>();
            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            };
            accessor.HttpContext.Returns(context);
            return accessor;
        }

        public static IHttpContextAccessor CreateNullContext()
        {
            var accessor = Substitute.For<IHttpContextAccessor>();
            accessor.HttpContext.Returns((HttpContext?)null);
            return accessor;
        }
    }
}
