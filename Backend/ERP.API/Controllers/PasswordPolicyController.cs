using ERP.Application.DTOs;
using ERP.Application.Interfaces.Services;
using ERP.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PasswordPolicyController : ControllerBase
{
    private readonly IPasswordPolicyService _passwordPolicyService;

    public PasswordPolicyController(IPasswordPolicyService passwordPolicyService)
    {
        _passwordPolicyService = passwordPolicyService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetPolicy()
    {
        var policy = _passwordPolicyService.GetPolicy();
        return Ok(policy);
    }

    [HttpPut]
    [Authorize(Policy = AppPolicies.Security.Edit)]
    public IActionResult UpdatePolicy([FromBody] UpdatePasswordPolicyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = _passwordPolicyService.UpdatePolicy(dto);
        return Ok(updated);
    }

    [HttpPost("validate")]
    [AllowAnonymous]
    public IActionResult ValidatePassword([FromBody] ValidatePasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { message = "Password cannot be empty." });
        }

        var result = _passwordPolicyService.ValidatePassword(request.Password, request.UserId);
        return Ok(result);
    }

    public class ValidatePasswordRequest
    {
        public string Password { get; set; } = string.Empty;
        public int? UserId { get; set; }
    }
}
