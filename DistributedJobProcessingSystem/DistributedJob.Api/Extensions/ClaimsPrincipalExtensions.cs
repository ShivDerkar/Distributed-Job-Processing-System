using System.Security.Claims;

namespace DistributedJob.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("User ID was not found in token.");
        }

        return Guid.Parse(userId);
    }
}