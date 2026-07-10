namespace ClothingERP.Application.DTOs;

public class CommissionSettingsDto
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; }
    public decimal DefaultCommissionPercent { get; set; }
    public decimal MinSaleAmountForCommission { get; set; }
    public bool ExcludeReturnsFromCommission { get; set; }
}

public class UpdateCommissionSettingsDto
{
    [Required] public bool IsEnabled { get; set; }
    [Required, Range(0, 100)] public decimal DefaultCommissionPercent { get; set; }
    [Required, Range(0, 100000)] public decimal MinSaleAmountForCommission { get; set; }
    [Required] public bool ExcludeReturnsFromCommission { get; set; }
}

public class StaffCommissionRateDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public decimal? CommissionPercent { get; set; }  // null = default rate ব্যবহার হচ্ছে
    public decimal EffectiveRate { get; set; }  // আসলে যেই rate apply হবে
    public bool IsCustomRate { get; set; }
}

public class SetStaffRateDto
{
    [Required] public int UserId { get; set; }
    [Required, Range(0, 100)] public decimal CommissionPercent { get; set; }
}

// ── Summary Report ───────────────────────────────────────────────────────
public class StaffCommissionSummaryDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public decimal EffectiveRate { get; set; }
    public int TotalSalesCount { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal PendingCommission { get; set; }
    public decimal PaidCommission { get; set; }
}

public class CommissionTransactionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int SalesInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal SaleAmount { get; set; }
    public decimal CommissionPercent { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public DateTime? PaidDate { get; set; }
}

public class MarkCommissionPaidDto
{
    [Required] public List<int> TransactionIds { get; set; } = new();
    public string? Notes { get; set; }
}