using Microsoft.AspNetCore.Authorization;

namespace ERP.API.Security;

/// <summary>
/// Authorization requirement representing a granular menu option permission check.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string MenuRoute { get; }
    public string OptionCode { get; }

    public PermissionRequirement(string menuRoute, string optionCode)
    {
        MenuRoute = menuRoute?.Trim().ToLowerInvariant() ?? string.Empty;
        OptionCode = optionCode?.Trim().ToUpperInvariant() ?? string.Empty;
    }
}
