using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ERP.API.Security;

/// <summary>
/// Dynamically resolves policies formatted as "{menuRoute}:{optionCode}" or "Permissions:{menuRoute}:{optionCode}"
/// without requiring static pre-registration of every route/option permutation in AddAuthorization.
/// </summary>
public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // 1. Check if policy is already registered in AuthorizationOptions (e.g. AdminOnly, etc.)
        var existingPolicy = await base.GetPolicyAsync(policyName);
        if (existingPolicy != null)
        {
            return existingPolicy;
        }

        // 2. Normalize and check for dynamic permission policy format
        var cleaned = policyName;
        if (cleaned.StartsWith("Permissions:", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned["Permissions:".Length..];
        }
        else if (cleaned.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned["Permission:".Length..];
        }

        var parts = cleaned.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
        {
            var menuRoute = parts[0].ToLowerInvariant();
            var optionCode = parts[1].ToUpperInvariant();

            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(menuRoute, optionCode))
                .Build();
        }

        return null;
    }
}
