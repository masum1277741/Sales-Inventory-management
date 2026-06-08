namespace ClothingERP.Application.DTOs;

public class CustomerListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public decimal LoyaltyPoints { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerDto : CustomerListDto
{
    public int CustomerGroupId { get; set; }
    public string? Address { get; set; }
    public string? NIDNumber { get; set; }
    public string? ProfileImage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCustomerDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? NIDNumber { get; set; }
    [Required]
    public int CustomerGroupId { get; set; }
    public string? ProfileImagePath { get; set; }
}

public class UpdateCustomerDto : CreateCustomerDto
{
    public bool IsActive { get; set; } = true;
}

public class CustomerGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public bool IsActive { get; set; }
    public int CustomerCount { get; set; }
}

public class CreateCustomerGroupDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [Range(0, 100)]
    public decimal DiscountPercentage { get; set; }
}

public class CustomerLedgerDto
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

public class CustomerDueDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public decimal DueAmount { get; set; }
}