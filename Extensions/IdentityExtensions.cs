using System.Security.Claims;

namespace school_diary.Extensions;

public static class IdentityExtensions
{
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)!;
}