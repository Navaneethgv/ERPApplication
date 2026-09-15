using System.Security.Claims;
using ERP.Application.Interfaces.Services;
using ERP.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace ERP.API.Security;

/// <summary>
/// Evaluates dynamic permissions for the current user's role against the active Menu Master and Role Permissions.
/// Superuser role (Admin) automatically bypasses restrictions.
/// Other roles query IRolePermissionService.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IRolePermissionService _rolePermissionService;

    public PermissionAuthorizationHandler(IRolePermissionService rolePermissionService)
    {
        _rolePermissionService = rolePermissionService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var user = context.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return Task.CompletedTask;
        }

        var roleStr = user.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(roleStr) || !Enum.TryParse<UserRole>(roleStr, true, out var role))
        {
            return Task.CompletedTask;
        }

        // Administrators possess full superuser access across all modules
        if (role == UserRole.Admin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check dynamic permission in database
        bool hasPerm = _rolePermissionService.HasPermission(role, requirement.MenuRoute, requirement.OptionCode);
        if (hasPerm)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
