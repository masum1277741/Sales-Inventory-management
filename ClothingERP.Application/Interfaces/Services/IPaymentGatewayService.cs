namespace ClothingERP.Application.Interfaces.Services;

public interface IPaymentGatewayService
{
    // ── POS থেকে call হবে — payment শুরু করার জন্য ──────────────────────────
    Task<InitiatePaymentResultDto> InitiatePaymentAsync(InitiatePaymentDto dto, int userId);

    // ── bKash flow এ customer approve করার পরে এই step লাগে ────────────────
    Task<ServiceResult<PaymentStatusDto>> ExecutePaymentAsync(ExecutePaymentDto dto);

    // ── POS পেইজ থেকে polling করে status চেক করার জন্য ──────────────────────
    Task<PaymentStatusDto> CheckStatusAsync(string gatewayPaymentId);

    // ── Callback URL এ gateway থেকে redirect এলে call হবে ───────────────────
    Task<ServiceResult<PaymentStatusDto>> HandleCallbackAsync(string provider, string gatewayPaymentId, string? status);

    Task<IEnumerable<DigitalPaymentTransaction>> GetRecentTransactionsAsync(int take = 50);
}