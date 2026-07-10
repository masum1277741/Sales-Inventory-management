namespace ClothingERP.Application.Services;

public class GiftCardService : IGiftCardService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GiftCardService(IUnitOfWork uow, IMapper mapper)
        => (_uow, _mapper) = (uow, mapper);

    // ── Unique Code Generator ─────────────────────────────────────────────
    private string GenerateCardCode()
    {
        var rnd = new Random();
        string Block() => rnd.Next(0, 9999).ToString("D4");
        return $"GC-{Block()}-{Block()}-{Block()}";
    }

    // ── Get All ───────────────────────────────────────────────────────────
    public async Task<IEnumerable<GiftCardListDto>> GetAllAsync()
    {
        var cards = await _uow.GiftCards.GetQueryable()
            .Include(g => g.IssuedToCustomer)
            .Where(g => !g.IsDeleted)
            .OrderByDescending(g => g.IssuedDate)
            .ToListAsync();

        return cards.Select(MapToListDto);
    }

    private GiftCardListDto MapToListDto(GiftCard g) => new()
    {
        Id = g.Id,
        CardCode = g.CardCode,
        InitialValue = g.InitialValue,
        CurrentBalance = g.CurrentBalance,
        CustomerName = g.IssuedToCustomer?.Name,
        RecipientName = g.RecipientName,
        IssuedDate = g.IssuedDate,
        ExpiryDate = g.ExpiryDate,
        Status = g.Status,
        IsStoreCredit = g.IsStoreCredit
    };

    // ── Get By Id (with history) ─────────────────────────────────────────
    public async Task<GiftCardDto?> GetByIdAsync(int id)
    {
        var card = await _uow.GiftCards.GetQueryable()
            .Include(g => g.IssuedToCustomer)
            .Include(g => g.Transactions)
            .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

        if (card == null) return null;

        var listDto = MapToListDto(card);
        return new GiftCardDto
        {
            Id = listDto.Id,
            CardCode = listDto.CardCode,
            InitialValue = listDto.InitialValue,
            CurrentBalance = listDto.CurrentBalance,
            CustomerName = listDto.CustomerName,
            RecipientName = listDto.RecipientName,
            IssuedDate = listDto.IssuedDate,
            ExpiryDate = listDto.ExpiryDate,
            Status = listDto.Status,
            IsStoreCredit = listDto.IsStoreCredit,
            Notes = card.Notes,
            Transactions = card.Transactions.OrderByDescending(t => t.TransactionDate).Select(t => new GiftCardTransactionDto
            {
                Id = t.Id,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                SalesInvoiceId = t.SalesInvoiceId,
                TransactionDate = t.TransactionDate,
                Notes = t.Notes
            }).ToList()
        };
    }

    // ── Issue New Purchased Gift Card ─────────────────────────────────────
    public async Task<ServiceResult<GiftCardDto>> IssueAsync(IssueGiftCardDto dto, int userId)
    {
        var card = new GiftCard
        {
            CardCode = GenerateCardCode(),
            InitialValue = dto.Amount,
            CurrentBalance = dto.Amount,
            IssuedToCustomerId = dto.CustomerId,
            RecipientName = dto.RecipientName,
            RecipientPhone = dto.RecipientPhone,
            ExpiryDate = dto.ExpiryDate,
            Status = "Active",
            IsStoreCredit = false,
            Notes = dto.Notes,
            CreatedBy = userId
        };
        await _uow.GiftCards.AddAsync(card);
        await _uow.SaveChangesAsync();

        await _uow.GiftCardTransactions.AddAsync(new GiftCardTransaction
        {
            GiftCardId = card.Id,
            TransactionType = "Issued",
            Amount = dto.Amount,
            BalanceAfter = dto.Amount,
            Notes = $"Purchased via {dto.PaymentMethod}",
            CreatedBy = userId
        });
        await _uow.SaveChangesAsync();

        var result = await GetByIdAsync(card.Id);
        return ServiceResult<GiftCardDto>.Ok(result!, $"Gift card {card.CardCode} issued successfully.");
    }

    // ── Issue Store Credit (Return/Refund থেকে) ───────────────────────────
    public async Task<ServiceResult<GiftCardDto>> IssueStoreCreditAsync(IssueStoreCreditDto dto, int userId)
    {
        var customer = await _uow.Customers.GetByIdAsync(dto.CustomerId);
        if (customer == null) return ServiceResult<GiftCardDto>.Fail("Customer not found.");

        var card = new GiftCard
        {
            CardCode = GenerateCardCode(),
            InitialValue = dto.Amount,
            CurrentBalance = dto.Amount,
            IssuedToCustomerId = dto.CustomerId,
            RecipientName = customer.Name,
            IssuedDate = DateTime.UtcNow,
            ExpiryDate = null,    // store credit সাধারণত expire হয় না
            Status = "Active",
            IsStoreCredit = true,
            SourceReturnId = dto.SourceReturnId,
            Notes = dto.Notes ?? "Issued as store credit from return/refund",
            CreatedBy = userId
        };
        await _uow.GiftCards.AddAsync(card);
        await _uow.SaveChangesAsync();

        await _uow.GiftCardTransactions.AddAsync(new GiftCardTransaction
        {
            GiftCardId = card.Id,
            TransactionType = "Issued",
            Amount = dto.Amount,
            BalanceAfter = dto.Amount,
            Notes = card.Notes,
            CreatedBy = userId
        });
        await _uow.SaveChangesAsync();

        var result = await GetByIdAsync(card.Id);
        return ServiceResult<GiftCardDto>.Ok(result!, $"Store credit issued: {card.CardCode} (${dto.Amount:N2})");
    }

    // ── Lookup (POS এর জন্য balance check) ────────────────────────────────
    public async Task<GiftCardLookupDto> LookupAsync(string cardCode)
    {
        var card = await _uow.GiftCards.GetQueryable()
            .FirstOrDefaultAsync(g => g.CardCode == cardCode.Trim().ToUpper() && !g.IsDeleted);

        if (card == null)
            return new GiftCardLookupDto { Found = false, Message = "Gift card / store credit code পাওয়া যায়নি।" };

        // Expiry check
        if (card.Status == "Active" && card.ExpiryDate.HasValue && card.ExpiryDate.Value.Date < DateTime.UtcNow.Date)
        {
            card.Status = "Expired";
            _uow.GiftCards.Update(card);
            await _uow.SaveChangesAsync();
        }

        var usable = card.Status == "Active" && card.CurrentBalance > 0;

        return new GiftCardLookupDto
        {
            Found = true,
            CardCode = card.CardCode,
            CurrentBalance = card.CurrentBalance,
            Status = card.Status,
            IsUsable = usable,
            Message = usable ? null :
                              card.Status == "Expired" ? "এই কার্ডের মেয়াদ শেষ হয়ে গেছে।" :
                              card.Status == "Cancelled" ? "এই কার্ড বাতিল করা হয়েছে।" :
                              card.CurrentBalance <= 0 ? "এই কার্ডে কোনো ব্যালেন্স নেই।" : null
        };
    }

    // ── Redeem (POS checkout এর সময় call হবে) ────────────────────────────
    public async Task<ServiceResult<decimal>> RedeemAsync(
        string cardCode, decimal amount, int? salesInvoiceId, int userId)
    {
        var card = await _uow.GiftCards.GetQueryable()
            .FirstOrDefaultAsync(g => g.CardCode == cardCode.Trim().ToUpper() && !g.IsDeleted);

        if (card == null) return ServiceResult<decimal>.Fail("Gift card পাওয়া যায়নি।");
        if (card.Status != "Active") return ServiceResult<decimal>.Fail($"এই কার্ড {card.Status} অবস্থায় আছে।");
        if (card.CurrentBalance <= 0) return ServiceResult<decimal>.Fail("কার্ডে কোনো ব্যালেন্স নেই।");

        // যতটুকু balance আছে তার বেশি নেওয়া যাবে না — actual deduct হবে min(amount, balance)
        var deductAmount = Math.Min(amount, card.CurrentBalance);

        card.CurrentBalance -= deductAmount;
        if (card.CurrentBalance <= 0) card.Status = "Depleted";
        card.UpdatedBy = userId;
        card.UpdatedAt = DateTime.UtcNow;
        _uow.GiftCards.Update(card);

        await _uow.GiftCardTransactions.AddAsync(new GiftCardTransaction
        {
            GiftCardId = card.Id,
            TransactionType = "Redeemed",
            Amount = -deductAmount,
            BalanceAfter = card.CurrentBalance,
            SalesInvoiceId = salesInvoiceId,
            CreatedBy = userId
        });
        await _uow.SaveChangesAsync();

        return ServiceResult<decimal>.Ok(deductAmount, "Gift card redeemed successfully.");
    }

    // ── Cancel ────────────────────────────────────────────────────────────
    public async Task<ServiceResult> CancelAsync(int id, int userId)
    {
        var card = await _uow.GiftCards.GetByIdAsync(id);
        if (card == null) return ServiceResult.Fail("Gift card not found.");
        if (card.Status == "Depleted") return ServiceResult.Fail("ইতিমধ্যে সম্পূর্ণ ব্যবহার হয়ে গেছে।");

        card.Status = "Cancelled";
        card.UpdatedBy = userId;
        card.UpdatedAt = DateTime.UtcNow;
        _uow.GiftCards.Update(card);

        await _uow.GiftCardTransactions.AddAsync(new GiftCardTransaction
        {
            GiftCardId = id,
            TransactionType = "Cancelled",
            Amount = -card.CurrentBalance,
            BalanceAfter = 0,
            Notes = "Manually cancelled by admin",
            CreatedBy = userId
        });

        card.CurrentBalance = 0;
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok("Gift card cancelled successfully.");
    }

    // ── Customer এর সব store credit/gift card ─────────────────────────────
    public async Task<IEnumerable<GiftCardListDto>> GetCustomerCreditsAsync(int customerId)
    {
        var cards = await _uow.GiftCards.GetQueryable()
            .Where(g => !g.IsDeleted && g.IssuedToCustomerId == customerId &&
                        g.Status == "Active" && g.CurrentBalance > 0)
            .ToListAsync();
        return cards.Select(MapToListDto);
    }

    // ── Expire পুরনো card গুলো (background job এ চলবে) ────────────────────
    public async Task<int> ExpireOldCardsAsync()
    {
        var expired = await _uow.GiftCards.GetQueryable()
            .Where(g => !g.IsDeleted && g.Status == "Active" &&
                        g.ExpiryDate.HasValue && g.ExpiryDate.Value.Date < DateTime.UtcNow.Date)
            .ToListAsync();

        foreach (var card in expired)
        {
            card.Status = "Expired";
            _uow.GiftCards.Update(card);
        }
        if (expired.Any()) await _uow.SaveChangesAsync();
        return expired.Count;
    }
}