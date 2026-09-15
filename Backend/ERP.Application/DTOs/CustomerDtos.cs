using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

public class CustomerDto
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
}

public class CreateCustomerDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public string Company { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    [Range(0, 10000000)]
    public decimal CreditLimit { get; set; } = 10000m;

    public string Status { get; set; } = "Active";

    // Optional user login creation
    public bool CreateLoginAccount { get; set; } = false;
    public string? Password { get; set; }
}

public class UpdateCustomerDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public string Company { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    [Range(0, 10000000)]
    public decimal CreditLimit { get; set; }

    public string Status { get; set; } = "Active";
}

