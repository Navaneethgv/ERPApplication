using Microsoft.AspNetCore.Authorization;

namespace ERP.API.Security;

/// <summary>
/// Convenience attribute that maps directly to ASP.NET Core policy-based authorization
/// using the standard "{menuRoute}:{optionCode}" policy naming scheme.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    public string MenuRoute { get; }
    public string OptionCode { get; }

    public HasPermissionAttribute(string menuRoute, string optionCode)
    {
        MenuRoute = menuRoute?.Trim().ToLowerInvariant() ?? string.Empty;
        OptionCode = optionCode?.Trim().ToUpperInvariant() ?? string.Empty;
        Policy = $"{MenuRoute}:{OptionCode}";
    }
}
