namespace ClothingERP.Application.Interfaces.Services;

public interface IGiftCardService
{
    Task<IEnumerable<GiftCardListDto>> GetAllAsync();
    Task<GiftCardDto?> GetByIdAsync(int id);

    Task<ServiceResult<GiftCardDto>> IssueAsync(IssueGiftCardDto dto, int userId);
    Task<ServiceResult<GiftCardDto>> IssueStoreCreditAsync(IssueStoreCreditDto dto, int userId);

    Task<GiftCardLookupDto> LookupAsync(string cardCode);
    Task<ServiceResult<decimal>> RedeemAsync(string cardCode, decimal amount, int? salesInvoiceId, int userId);

    Task<ServiceResult> CancelAsync(int id, int userId);
    Task<IEnumerable<GiftCardListDto>> GetCustomerCreditsAsync(int customerId);
    Task<int> ExpireOldCardsAsync(); // expired card গুলো status আপডেট করার জন্য (background job এ ব্যবহার হবে)
}