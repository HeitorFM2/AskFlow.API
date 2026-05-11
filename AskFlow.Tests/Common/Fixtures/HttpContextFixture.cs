using AskFlow.Application.Interfaces;

namespace AskFlow.Tests.Common.Fixtures
{
    public static class HttpContextFixture
    {
        public static ICurrentUserService CreateAuthenticated(string userId)
        {
            var service = Substitute.For<ICurrentUserService>();
            service.GetUserId().Returns(userId);
            return service;
        }

        public static ICurrentUserService CreateUnauthenticated()
        {
            var service = Substitute.For<ICurrentUserService>();
            service.GetUserId().Returns((string?)null);
            return service;
        }

        public static ICurrentUserService CreateNullContext()
        {
            var service = Substitute.For<ICurrentUserService>();
            service.GetUserId().Returns((string?)null);
            return service;
        }
    }
}
