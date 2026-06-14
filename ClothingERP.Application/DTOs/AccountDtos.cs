namespace ClothingERP.Application.DTOs;

public class AccountTransactionListDto
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
}

public class AccountTransactionDto : AccountTransactionListDto
{
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAccountTransactionDto
{
    [Required]
    public TransactionType TransactionType { get; set; }
    [Required]
    public AccountCategory Category { get; set; }
    [Required, Range(0.01, 9999999)]
    public decimal Amount { get; set; }
    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.Today;
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}