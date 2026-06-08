namespace ClothingERP.Application.DTOs;

public class SupplierListDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
}

public class SupplierDto : SupplierListDto
{
    public string? Address { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSupplierDto
{
    [Required, MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string ContactPerson { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    [EmailAddress]
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
}

public class UpdateSupplierDto : CreateSupplierDto
{
    public bool IsActive { get; set; } = true;
}

public class SupplierLedgerDto
{
    public int Id { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public DateTime EntryDate { get; set; }
}

public class SupplierDueDto
{
    public int SupplierId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal DueAmount { get; set; }
}