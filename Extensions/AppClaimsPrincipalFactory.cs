using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using school_diary.Models;

namespace school_diary.Extensions;

public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<User>
{
    public AppClaimsPrincipalFactory(
        UserManager<User> userManager,
        IOptions<IdentityOptions> options)
        : base(userManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var id = await base.GenerateClaimsAsync(user);

        id.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
        id.AddClaim(new Claim("FullName", $"{user.FirstName} {user.LastName}"));

        return id;
    }
}