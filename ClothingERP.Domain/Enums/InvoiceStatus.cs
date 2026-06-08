namespace ClothingERP.Domain.Enums;

public enum InvoiceStatus
{
    Draft = 1,
    Confirmed = 2,
    PartiallyPaid = 3,
    FullyPaid = 4,
    Cancelled = 5,
    Hold = 6
}