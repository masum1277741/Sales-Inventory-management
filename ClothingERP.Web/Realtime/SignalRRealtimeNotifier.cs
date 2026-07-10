using ClothingERP.Application.Interfaces.Services;
using ClothingERP.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ClothingERP.Web.Realtime;

public class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<AppHub> _hub;

    public SignalRRealtimeNotifier(IHubContext<AppHub> hub) => _hub = hub;

    public async Task NotifyStockUpdatedAsync(int variantId, string barcode, int newQuantity, string productName)
    {
        await _hub.Clients.All.SendAsync("StockUpdated", new
        {
            variantId,
            barcode,
            quantity = newQuantity,
            productName
        });
    }

    public async Task NotifySaleCompletedAsync(decimal totalAmount, string invoiceNumber)
    {
        await _hub.Clients.All.SendAsync("SaleCompleted", new { totalAmount, invoiceNumber });
    }

    public async Task NotifyLowStockAsync(int variantId, string productName, int quantity)
    {
        await _hub.Clients.All.SendAsync("LowStockAlert", new { variantId, productName, quantity });
    }
}