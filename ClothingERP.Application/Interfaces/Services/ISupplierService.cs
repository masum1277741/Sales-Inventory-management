namespace ClothingERP.Application.Interfaces.Services;

public interface ISupplierService
{
    Task<IEnumerable<SupplierListDto>> GetAllAsync();
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<ServiceResult<SupplierDto>> CreateAsync(CreateSupplierDto dto, int userId);
    Task<ServiceResult<SupplierDto>> UpdateAsync(int id, UpdateSupplierDto dto, int userId);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> ToggleStatusAsync(int id, int userId);

    // Ledger & Payments
    Task<IEnumerable<SupplierLedgerDto>> GetLedgerAsync(int supplierId, DateTime? from = null, DateTime? to = null);
    Task<decimal> GetBalanceAsync(int supplierId);
    Task<ServiceResult> AddPaymentAsync(int supplierId, decimal amount,
                                        PaymentMethod method, string? reference, int userId);
}