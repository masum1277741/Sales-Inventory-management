namespace ClothingERP.Application.Services;

public class LoyaltyService : ILoyaltyService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public LoyaltyService(IUnitOfWork uow, IMapper mapper)
        => (_uow, _mapper) = (uow, mapper);

    // ── Settings ──────────────────────────────────────────────────────────
    public async Task<LoyaltySettingsDto> GetSettingsAsync()
    {
        var settings = (await _uow.LoyaltySettings.GetAllAsync()).FirstOrDefault();

        // প্রথমবার হলে default settings তৈরি করো
        if (settings == null)
        {
            settings = new LoyaltySettings();
            await _uow.LoyaltySettings.AddAsync(settings);
            await _uow.SaveChangesAsync();
        }

        return _mapper.Map<LoyaltySettingsDto>(settings);
    }

    public async Task<ServiceResult> UpdateSettingsAsync(UpdateLoyaltySettingsDto dto, int userId)
    {
        var settings = (await _uow.LoyaltySettings.GetAllAsync()).FirstOrDefault();
        if (settings == null)
        {
            settings = new LoyaltySettings();
            await _uow.LoyaltySettings.AddAsync(settings);
        }

        settings.IsEnabled = dto.IsEnabled;
        settings.PointsPerDollarSpent = dto.PointsPerDollarSpent;
        settings.PointValueInDollars = dto.PointValueInDollars;
        settings.MinPointsToRedeem = dto.MinPointsToRedeem;
        settings.BirthdayBonusPoints = dto.BirthdayBonusPoints;
        settings.UpdatedBy = userId;
        settings.UpdatedAt = DateTime.UtcNow;

        _uow.LoyaltySettings.Update(settings);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok("Loyalty settings updated successfully.");
    }

    // ── Customer Loyalty Info ────────────────────────────────────────────
    public async Task<CustomerLoyaltyDto> GetCustomerLoyaltyAsync(int customerId)
    {
        var customer = await _uow.Customers.GetByIdAsync(customerId)
            ?? throw new InvalidOperationException("Customer not found.");

        var settings = await GetSettingsAsync();

        var history = await _uow.LoyaltyTransactions.GetQueryable()
            .Where(t => t.CustomerId == customerId && !t.IsDeleted)
            .OrderByDescending(t => t.TransactionDate)
            .Take(10)
            .ToListAsync();

        var historyDtos = history.Select(h => new LoyaltyTransactionDto
        {
            Id = h.Id,
            TransactionType = h.TransactionType,
            Points = h.Points,
            Description = h.Description,
            SalesInvoiceId = h.SalesInvoiceId,
            TransactionDate = h.TransactionDate,
            BalanceAfter = h.BalanceAfter
        }).ToList();

        return new CustomerLoyaltyDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CurrentPoints = (int)customer.LoyaltyPoints,
            RedeemableValueUSD = customer.LoyaltyPoints * settings.PointValueInDollars,
            CanRedeem = settings.IsEnabled && customer.LoyaltyPoints >= settings.MinPointsToRedeem,
            RecentHistory = historyDtos
        };
    }

    // ── Award Points (sale সম্পন্ন হলে call হবে) ─────────────────────────
    public async Task AwardPointsAsync(int customerId, decimal saleAmount, int? salesInvoiceId, int userId)
    {
        var settings = await GetSettingsAsync();
        if (!settings.IsEnabled || saleAmount <= 0) return;

        var customer = await _uow.Customers.GetByIdAsync(customerId);
        if (customer == null) return;

        var pointsEarned = (int)Math.Floor(saleAmount * settings.PointsPerDollarSpent);
        if (pointsEarned <= 0) return;

        customer.LoyaltyPoints += pointsEarned;
        customer.UpdatedBy = userId;
        customer.UpdatedAt = DateTime.UtcNow;
        _uow.Customers.Update(customer);

        var txn = new LoyaltyTransaction
        {
            CustomerId = customerId,
            TransactionType = "Earned",
            Points = pointsEarned,
            Description = $"Earned from sale (${saleAmount:N2})",
            SalesInvoiceId = salesInvoiceId,
            BalanceAfter = (int)customer.LoyaltyPoints,
            CreatedBy = userId
        };
        await _uow.LoyaltyTransactions.AddAsync(txn);
        await _uow.SaveChangesAsync();
    }

    // ── Redeem Preview (POS এ amount দেখানোর জন্য, কিছু save হয় না) ──────
    public async Task<RedeemPreviewDto> PreviewRedeemAsync(int customerId, int pointsToRedeem)
    {
        var settings = await GetSettingsAsync();
        var customer = await _uow.Customers.GetByIdAsync(customerId);

        if (customer == null)
            return new RedeemPreviewDto { Success = false, Message = "Customer not found." };

        if (!settings.IsEnabled)
            return new RedeemPreviewDto { Success = false, Message = "Loyalty program is currently disabled." };

        if (pointsToRedeem < settings.MinPointsToRedeem)
            return new RedeemPreviewDto { Success = false, Message = $"Minimum {settings.MinPointsToRedeem} points required to redeem." };

        if (pointsToRedeem > customer.LoyaltyPoints)
            return new RedeemPreviewDto { Success = false, Message = "Customer doesn't have enough points." };

        return new RedeemPreviewDto
        {
            Success = true,
            PointsToRedeem = pointsToRedeem,
            DiscountValue = pointsToRedeem * settings.PointValueInDollars
        };
    }

    // ── Actually Redeem (CreateInvoice flow এর ভেতর থেকে call হবে) ───────
    public async Task<ServiceResult<decimal>> RedeemPointsAsync(
        int customerId, int pointsToRedeem, int? salesInvoiceId, int userId)
    {
        var preview = await PreviewRedeemAsync(customerId, pointsToRedeem);
        if (!preview.Success)
            return ServiceResult<decimal>.Fail(preview.Message!);

        var customer = await _uow.Customers.GetByIdAsync(customerId);
        customer!.LoyaltyPoints -= pointsToRedeem;
        customer.UpdatedBy = userId;
        customer.UpdatedAt = DateTime.UtcNow;
        _uow.Customers.Update(customer);

        var txn = new LoyaltyTransaction
        {
            CustomerId = customerId,
            TransactionType = "Redeemed",
            Points = -pointsToRedeem,
            Description = $"Redeemed for ${preview.DiscountValue:N2} discount",
            SalesInvoiceId = salesInvoiceId,
            BalanceAfter = (int)customer.LoyaltyPoints,
            CreatedBy = userId
        };
        await _uow.LoyaltyTransactions.AddAsync(txn);
        await _uow.SaveChangesAsync();

        return ServiceResult<decimal>.Ok(preview.DiscountValue, "Points redeemed successfully.");
    }

    // ── Birthday Bonus (manual trigger — পরে background job এ যাবে) ─────
    public async Task<int> ApplyBirthdayBonusesAsync(int userId)
    {
        var settings = await GetSettingsAsync();
        if (!settings.IsEnabled || settings.BirthdayBonusPoints <= 0) return 0;

        var today = DateTime.Today;
        var customers = (await _uow.Customers.GetAllAsync())
            .Where(c => c.IsActive &&
                        c.DateOfBirth.HasValue &&
                        c.DateOfBirth.Value.Month == today.Month &&
                        c.DateOfBirth.Value.Day == today.Day)
            .ToList();

        int count = 0;
        foreach (var customer in customers)
        {
            // আজকে আগেই bonus দেওয়া হয়েছে কিনা চেক করো (duplicate প্রতিরোধ)
            var alreadyGiven = await _uow.LoyaltyTransactions.GetQueryable()
                .AnyAsync(t => t.CustomerId == customer.Id &&
                               t.TransactionType == "Bonus" &&
                               t.TransactionDate.Date == today);
            if (alreadyGiven) continue;

            customer.LoyaltyPoints += settings.BirthdayBonusPoints;
            customer.UpdatedBy = userId;
            customer.UpdatedAt = DateTime.UtcNow;
            _uow.Customers.Update(customer);

            await _uow.LoyaltyTransactions.AddAsync(new LoyaltyTransaction
            {
                CustomerId = customer.Id,
                TransactionType = "Bonus",
                Points = settings.BirthdayBonusPoints,
                Description = "Happy Birthday bonus points!",
                BalanceAfter = (int)customer.LoyaltyPoints,
                CreatedBy = userId
            });
            count++;
        }

        if (count > 0) await _uow.SaveChangesAsync();
        return count;
    }
}