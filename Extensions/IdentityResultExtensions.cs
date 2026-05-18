using Microsoft.AspNetCore.Identity;

namespace school_diary.Extensions;

public static class IdentityResultExtensions
{
    public static void CheckErrors(this IdentityResult result)
    {
        if (result.Succeeded) return;

        var msg = string.Join("; ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException(msg);
    }
}