namespace ClothingERP.Application.Interfaces.Services;

public interface ILoyaltyService
{
    Task<LoyaltySettingsDto> GetSettingsAsync();
    Task<ServiceResult> UpdateSettingsAsync(UpdateLoyaltySettingsDto dto, int userId);

    Task<CustomerLoyaltyDto> GetCustomerLoyaltyAsync(int customerId);

    // POS checkout এর সময় points award করার জন্য
    Task AwardPointsAsync(int customerId, decimal saleAmount, int? salesInvoiceId, int userId);

    // POS এ redeem করার আগে preview দেখানোর জন্য
    Task<RedeemPreviewDto> PreviewRedeemAsync(int customerId, int pointsToRedeem);

    // আসলে redeem করে discount apply করার জন্য (CreateAsync এর ভেতর থেকে call হবে)
    Task<ServiceResult<decimal>> RedeemPointsAsync(int customerId, int pointsToRedeem, int? salesInvoiceId, int userId);

    // Birthday bonus — background job থেকে call হবে (Feature #12/13 এ আসবে), আপাতত manual trigger
    Task<int> ApplyBirthdayBonusesAsync(int userId);
}