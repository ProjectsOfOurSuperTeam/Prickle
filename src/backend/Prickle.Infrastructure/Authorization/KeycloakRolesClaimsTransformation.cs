using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Prickle.Infrastructure.Authorization;

public class KeycloakRolesClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var claimsIdentity = principal.Identity as ClaimsIdentity;

        // Find the resource_access claim
        var resourceAccessClaim = principal.FindFirst("resource_access");

        if (resourceAccessClaim != null)
        {
            try
            {
                var resourceAccess = JsonDocument.Parse(resourceAccessClaim.Value);

                // Extract roles from resource_access.api.roles
                if (resourceAccess.RootElement.TryGetProperty("api", out var apiElement))
                {
                    if (apiElement.TryGetProperty("roles", out var rolesElement))
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (!string.IsNullOrEmpty(roleValue))
                            {
                                claimsIdentity?.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Handle parsing errors if needed
            }
        }

        return Task.FromResult(principal);
    }
}