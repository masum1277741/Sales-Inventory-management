namespace ClothingERP.Application.DTOs;

public class InitiatePaymentDto
{
    [Required] public string Provider { get; set; } = string.Empty;   // "bKash" | "Nagad"
    [Required, Range(0.01, 100000)] public decimal AmountUSD { get; set; }
    public string? CustomerMsisdn { get; set; }
}

public class InitiatePaymentResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string GatewayPaymentId { get; set; } = string.Empty;
    public string? RedirectUrl { get; set; }   // bKash এ checkout URL (browser এ open হবে)
    public string? QrCodeData { get; set; }   // চাইলে QR বানানো যাবে (এই string টা encode করে)
    public decimal AmountBDT { get; set; }
}

public class PaymentStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? GatewayTrxId { get; set; }
    public decimal AmountUSD { get; set; }
    public string? FailureReason { get; set; }
    public bool IsFinal { get; set; }   // true হলে polling বন্ধ করা যাবে (Completed/Failed/Cancelled)
}

public class ExecutePaymentDto
{
    [Required] public string GatewayPaymentId { get; set; } = string.Empty;
    [Required] public string Provider { get; set; } = string.Empty;
}