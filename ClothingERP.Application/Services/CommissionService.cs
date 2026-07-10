namespace ClothingERP.Application.Services;

public class CommissionService : ICommissionService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CommissionService(IUnitOfWork uow, IMapper mapper)
        => (_uow, _mapper) = (uow, mapper);

    // ── Settings ──────────────────────────────────────────────────────────
    public async Task<CommissionSettingsDto> GetSettingsAsync()
    {
        var settings = (await _uow.CommissionSettings.GetAllAsync()).FirstOrDefault();
        if (settings == null)
        {
            settings = new CommissionSettings();
            await _uow.CommissionSettings.AddAsync(settings);
            await _uow.SaveChangesAsync();
        }
        return _mapper.Map<CommissionSettingsDto>(settings);
    }

    public async Task<ServiceResult> UpdateSettingsAsync(UpdateCommissionSettingsDto dto, int userId)
    {
        var settings = (await _uow.CommissionSettings.GetAllAsync()).FirstOrDefault();
        if (settings == null) { settings = new CommissionSettings(); await _uow.CommissionSettings.AddAsync(settings); }

        settings.IsEnabled = dto.IsEnabled;
        settings.DefaultCommissionPercent = dto.DefaultCommissionPercent;
        settings.MinSaleAmountForCommission = dto.MinSaleAmountForCommission;
        settings.ExcludeReturnsFromCommission = dto.ExcludeReturnsFromCommission;
        settings.UpdatedBy = userId;
        settings.UpdatedAt = DateTime.UtcNow;

        _uow.CommissionSettings.Update(settings);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Commission settings updated successfully.");
    }

    // ── Staff Rates (sob active staff + তাদের override থাকলে দেখাবে) ─────
    public async Task<IEnumerable<StaffCommissionRateDto>> GetStaffRatesAsync()
    {
        var settings = await GetSettingsAsync();
        var users = await _uow.Users.GetQueryable()
            .Include(u => u.Role)
            .Where(u => u.IsActive && !u.IsDeleted)
            .ToListAsync();

        var overrides = await _uow.StaffCommissionRates.GetQueryable()
            .Where(r => !r.IsDeleted && r.IsActive)
            .ToListAsync();

        return users.Select(u =>
        {
            var ov = overrides.FirstOrDefault(o => o.UserId == u.Id);
            return new StaffCommissionRateDto
            {
                UserId = u.Id,
                UserName = u.FullName,
                RoleName = u.Role.Name,
                CommissionPercent = ov?.CommissionPercent,
                EffectiveRate = ov?.CommissionPercent ?? settings.DefaultCommissionPercent,
                IsCustomRate = ov != null
            };
        }).OrderBy(u => u.UserName).ToList();
    }

    public async Task<ServiceResult> SetStaffRateAsync(SetStaffRateDto dto, int userId)
    {
        var existing = await _uow.StaffCommissionRates.GetQueryable()
            .FirstOrDefaultAsync(r => r.UserId == dto.UserId && !r.IsDeleted);

        if (existing != null)
        {
            existing.CommissionPercent = dto.CommissionPercent;
            existing.IsActive = true;
            existing.UpdatedBy = userId;
            existing.UpdatedAt = DateTime.UtcNow;
            _uow.StaffCommissionRates.Update(existing);
        }
        else
        {
            await _uow.StaffCommissionRates.AddAsync(new StaffCommissionRate
            {
                UserId = dto.UserId,
                CommissionPercent = dto.CommissionPercent,
                IsActive = true,
                CreatedBy = userId
            });
        }
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Custom commission rate set successfully.");
    }

    public async Task<ServiceResult> RemoveStaffRateOverrideAsync(int userId)
    {
        var existing = await _uow.StaffCommissionRates.GetQueryable()
            .FirstOrDefaultAsync(r => r.UserId == userId && !r.IsDeleted);
        if (existing == null) return ServiceResult.Fail("কোনো custom rate পাওয়া যায়নি।");

        _uow.StaffCommissionRates.Remove(existing);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Default rate এ ফিরে যাওয়া হলো।");
    }

    // ── Helper: একটা specific staff এর effective rate বের করো ────────────
    private async Task<decimal> GetEffectiveRateAsync(int staffUserId)
    {
        var settings = await GetSettingsAsync();
        var ov = await _uow.StaffCommissionRates.GetQueryable()
            .FirstOrDefaultAsync(r => r.UserId == staffUserId && r.IsActive && !r.IsDeleted);
        return ov?.CommissionPercent ?? settings.DefaultCommissionPercent;
    }

    // ── Calculate & Record (Sale invoice তৈরি হওয়ার পরে call হবে) ─────────
    public async Task CalculateAndRecordCommissionAsync(
        int staffUserId, int salesInvoiceId, decimal saleAmount, int userId)
    {
        var settings = await GetSettingsAsync();
        if (!settings.IsEnabled) return;
        if (saleAmount < settings.MinSaleAmountForCommission) return;

        var rate = await GetEffectiveRateAsync(staffUserId);
        if (rate <= 0) return;

        var amount = Math.Round(saleAmount * (rate / 100m), 2);

        await _uow.CommissionTransactions.AddAsync(new CommissionTransaction
        {
            UserId = staffUserId,
            SalesInvoiceId = salesInvoiceId,
            SaleAmount = saleAmount,
            CommissionPercent = rate,
            CommissionAmount = amount,
            Status = "Pending",
            CreatedBy = userId
        });
        await _uow.SaveChangesAsync();
    }

    // ── Reverse (Sale cancel হলে call হবে) ────────────────────────────────
    public async Task ReverseCommissionAsync(int salesInvoiceId, int userId)
    {
        var txn = await _uow.CommissionTransactions.GetQueryable()
            .FirstOrDefaultAsync(c => c.SalesInvoiceId == salesInvoiceId && c.Status != "Reversed");
        if (txn == null) return;

        txn.Status = "Reversed";
        txn.Notes = "Sale was cancelled/returned";
        txn.UpdatedBy = userId;
        txn.UpdatedAt = DateTime.UtcNow;
        _uow.CommissionTransactions.Update(txn);
        await _uow.SaveChangesAsync();
    }

    // ── Summary Report (সব staff, তারিখ অনুযায়ী) ──────────────────────────
    public async Task<IEnumerable<StaffCommissionSummaryDto>> GetSummaryAsync(DateTime from, DateTime to)
    {
        var staffRates = (await GetStaffRatesAsync()).ToList();

        var txns = await _uow.CommissionTransactions.GetQueryable()
            .Where(c => !c.IsDeleted && c.Status != "Reversed" &&
                        c.TransactionDate.Date >= from.Date && c.TransactionDate.Date <= to.Date)
            .ToListAsync();

        var result = new List<StaffCommissionSummaryDto>();
        foreach (var staff in staffRates)
        {
            var staffTxns = txns.Where(t => t.UserId == staff.UserId).ToList();
            if (!staffTxns.Any()) continue;

            result.Add(new StaffCommissionSummaryDto
            {
                UserId = staff.UserId,
                UserName = staff.UserName,
                RoleName = staff.RoleName,
                EffectiveRate = staff.EffectiveRate,
                TotalSalesCount = staffTxns.Count,
                TotalSalesAmount = staffTxns.Sum(t => t.SaleAmount),
                TotalCommission = staffTxns.Sum(t => t.CommissionAmount),
                PendingCommission = staffTxns.Where(t => t.Status == "Pending").Sum(t => t.CommissionAmount),
                PaidCommission = staffTxns.Where(t => t.Status == "Paid").Sum(t => t.CommissionAmount)
            });
        }
        return result.OrderByDescending(r => r.TotalCommission);
    }

    // ── Individual Staff History ──────────────────────────────────────────
    public async Task<IEnumerable<CommissionTransactionDto>> GetStaffHistoryAsync(int userId, DateTime from, DateTime to)
    {
        var txns = await _uow.CommissionTransactions.GetQueryable()
            .Include(c => c.User)
            .Include(c => c.SalesInvoice)
            .Where(c => !c.IsDeleted && c.UserId == userId &&
                        c.TransactionDate.Date >= from.Date && c.TransactionDate.Date <= to.Date)
            .OrderByDescending(c => c.TransactionDate)
            .ToListAsync();

        return txns.Select(t => new CommissionTransactionDto
        {
            Id = t.Id,
            UserId = t.UserId,
            UserName = t.User.FullName,
            SalesInvoiceId = t.SalesInvoiceId,
            InvoiceNumber = t.SalesInvoice.InvoiceNumber,
            SaleAmount = t.SaleAmount,
            CommissionPercent = t.CommissionPercent,
            CommissionAmount = t.CommissionAmount,
            Status = t.Status,
            TransactionDate = t.TransactionDate,
            PaidDate = t.PaidDate
        });
    }

    // ── Mark As Paid (Payroll processing) ─────────────────────────────────
    public async Task<ServiceResult> MarkAsPaidAsync(MarkCommissionPaidDto dto, int paidByUserId)
    {
        if (!dto.TransactionIds.Any())
            return ServiceResult.Fail("কোনো transaction সিলেক্ট করা হয়নি।");

        var txns = await _uow.CommissionTransactions.GetQueryable()
            .Where(c => dto.TransactionIds.Contains(c.Id) && c.Status == "Pending")
            .ToListAsync();

        if (!txns.Any())
            return ServiceResult.Fail("কোনো pending commission পাওয়া যায়নি।");

        foreach (var txn in txns)
        {
            txn.Status = "Paid";
            txn.PaidDate = DateTime.UtcNow;
            txn.PaidBy = paidByUserId;
            txn.Notes = dto.Notes;
            txn.UpdatedBy = paidByUserId;
            txn.UpdatedAt = DateTime.UtcNow;
            _uow.CommissionTransactions.Update(txn);
        }
        await _uow.SaveChangesAsync();

        var total = txns.Sum(t => t.CommissionAmount);
        return ServiceResult.Ok($"{txns.Count} commission(s) totaling ${total:N2} marked as paid.");
    }
}