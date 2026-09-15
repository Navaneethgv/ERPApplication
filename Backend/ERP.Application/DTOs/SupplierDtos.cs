using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

public class SupplierDto
{
    public int SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = "Net 30";
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
}

public class CreateSupplierDto
{
    [Required]
    public string SupplierCode { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = "Net 30";
    public string Status { get; set; } = "Active";
}

public class UpdateSupplierDto
{
    [Required]
    public string SupplierCode { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = "Net 30";
    public string Status { get; set; } = "Active";
}

