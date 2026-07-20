namespace ClothingERP.Application.DTOs;

public class OfflineSaleSyncDto
{
    [Required] public string ClientTransactionId { get; set; } = string.Empty;
    [Required] public CreateSalesInvoiceDto Invoice { get; set; } = null!;
    [Required] public DateTime OriginalTimestamp { get; set; }  
}

public class SyncBatchResultDto
{
    public List<SyncItemResultDto> Results { get; set; } = new();
    public int SuccessCount => Results.Count(r => r.Status == "Synced");
    public int ConflictCount => Results.Count(r => r.Status == "Conflict");
    public int ErrorCount => Results.Count(r => r.Status == "Error");
}

public class SyncItemResultDto
{
    public string ClientTransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Synced | Conflict | Error | AlreadySynced
    public string? Message { get; set; }
    public int? ServerInvoiceId { get; set; }
    public string? ServerInvoiceNumber { get; set; }
    public List<ConflictItemDto>? Conflicts { get; set; }
}

public class ConflictItemDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQty { get; set; }
    public int AvailableQty { get; set; }
}