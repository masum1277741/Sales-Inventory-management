namespace ClothingERP.Application.Interfaces.Services;

public interface IRealtimeNotifier
{
    Task NotifyStockUpdatedAsync(int variantId, string barcode, int newQuantity, string productName);
    Task NotifySaleCompletedAsync(decimal totalAmount, string invoiceNumber);
    Task NotifyLowStockAsync(int variantId, string productName, int quantity);
}