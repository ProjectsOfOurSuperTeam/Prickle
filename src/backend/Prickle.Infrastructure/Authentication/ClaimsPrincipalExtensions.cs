using System.Security.Claims;

namespace Prickle.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out Guid parsedUserId)
            ? parsedUserId
            : throw new ApplicationException("User id is unavailable");
    }

    public static string GetUserEmail(this ClaimsPrincipal? principal)
    {
        var email = principal?.FindFirstValue(ClaimTypes.Email);

        return !string.IsNullOrWhiteSpace(email)
            ? email
            : throw new ApplicationException("User email is unavailable");
    }
}
