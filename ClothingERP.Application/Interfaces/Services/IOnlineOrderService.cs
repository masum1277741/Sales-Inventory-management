namespace ClothingERP.Application.Interfaces.Services;

public interface IOnlineOrderService
{
    Task<CartPricingResultDto> PriceCartAsync(CartPricingRequestDto dto, string currency);
    Task<ServiceResult<OrderConfirmationDto>> CheckoutAsync(CheckoutDto dto, int? customerId, int? userId);

    // ── Admin ──────────────────────────────────────────────────────────────
    Task<IEnumerable<OnlineOrderListDto>> GetAllAsync(string? statusFilter = null);
    Task<OnlineOrderDetailDto?> GetByIdAsync(int id);
    Task<ServiceResult> UpdateStatusAsync(UpdateOrderStatusDto dto, int userId);
}