using System.ComponentModel.DataAnnotations;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs;

public class EmployeeDto
{
    public int EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime JoiningDate { get; set; }
    public string Status { get; set; } = "Active";
}

public class CreateEmployeeDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string Designation { get; set; } = string.Empty;

    [Range(0, 10000000)]
    public decimal Salary { get; set; }

    public DateTime JoiningDate { get; set; } = DateTime.UtcNow.Date;

    public string Status { get; set; } = "Active";

    // Optional user login creation
    public bool CreateLoginAccount { get; set; } = false;
    public string? Password { get; set; }
}

public class UpdateEmployeeDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string Designation { get; set; } = string.Empty;

    [Range(0, 10000000)]
    public decimal Salary { get; set; }

    public DateTime JoiningDate { get; set; }

    public string Status { get; set; } = "Active";
}

