using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Infrastructure.Services
{
    public class IdentityPasswordSignInService(SignInManager<User> signInManager) : IPasswordSignInService
    {
        public async Task<PasswordSignInResult> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken)
        {
            var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return PasswordSignInResult.LockedOut;

            if (!result.Succeeded)
                return PasswordSignInResult.Failed;

            return PasswordSignInResult.Success;
        }
    }
}
