namespace ClothingERP.Domain.Entities;

public class DigitalPaymentTransaction : BaseEntity
{
    public string Provider { get; set; } = string.Empty;  // "bKash" | "Nagad"
    public string GatewayPaymentId { get; set; } = string.Empty;  // bKash/Nagad এর নিজস্ব paymentID
    public string? GatewayTrxId { get; set; }                  // সফল হলে gateway এর transaction id
    public decimal Amount { get; set; }                  // USD হিসাবে (POS এর base currency)
    public decimal AmountBDT { get; set; }                  // gateway কে যেটা পাঠানো হয়েছে (BDT, তারাও BDT ব্যবহার করে)
    public string Status { get; set; } = "Initiated";   // Initiated | Pending | Completed | Failed | Cancelled
    public int? SalesInvoiceId { get; set; }                  // success হলে কোন invoice এর সাথে যুক্ত
    public string? CustomerMsisdn { get; set; }                  // কাস্টমারের bKash/Nagad নম্বর
    public string? FailureReason { get; set; }
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? RawResponseJson { get; set; }                  // debugging/audit এর জন্য সম্পূর্ণ response

    public SalesInvoice? SalesInvoice { get; set; }
}