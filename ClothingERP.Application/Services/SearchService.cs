namespace ClothingERP.Application.Services;

public class SearchService : ISearchService
{
    private readonly IUnitOfWork _uow;
    private const int MaxPerCategory = 5;

    public SearchService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<GlobalSearchResultDto>> GlobalSearchAsync(string keyword, int currentUserId)
    {
        var results = new List<GlobalSearchResultDto>();
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 2)
            return results;

        keyword = keyword.Trim();

        // ── Products / Variants ──────────────────────────────────────────────
        var variants = await _uow.ProductVariants.GetAllWithDetailsAsync();
        var productMatches = variants
            .Where(v => v.IsActive && !v.IsDeleted &&
                       (v.Product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        v.Barcode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        v.Product.SKU.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .Take(MaxPerCategory)
            .Select(v => new SearchResultItemDto
            {
                Title = v.Product.Name,
                Subtitle = $"{v.Size.Name} · {v.Color.Name} · {v.Barcode}",
                Url = $"/Product/Details/{v.ProductId}",
                Badge = (v.Stock?.Quantity ?? 0) > 0 ? $"{v.Stock?.Quantity} in stock" : "Out of stock",
                BadgeColor = (v.Stock?.Quantity ?? 0) > 0 ? "success" : "danger"
            }).ToList();

        if (productMatches.Any())
            results.Add(new GlobalSearchResultDto { Category = "Products", Icon = "bi-box-seam", Items = productMatches });

        // ── Customers ─────────────────────────────────────────────────────────
        var customers = await _uow.Customers.GetQueryable()
            .Where(c => !c.IsDeleted &&
                       (c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        (c.PhoneNumber != null && c.PhoneNumber.Contains(keyword))))
            .Take(MaxPerCategory)
            .ToListAsync();

        if (customers.Any())
            results.Add(new GlobalSearchResultDto
            {
                Category = "Customers",
                Icon = "bi-person-lines-fill",
                Items = customers.Select(c => new SearchResultItemDto
                {
                    Title = c.Name,
                    Subtitle = c.PhoneNumber ?? "No phone",
                    Url = $"/Customer/Ledger/{c.Id}",
                    Badge = c.CurrentBalance > 0 ? $"Due ${c.CurrentBalance:N2}" : "No due",
                    BadgeColor = c.CurrentBalance > 0 ? "danger" : "success"
                }).ToList()
            });

        // ── Sales Invoices ───────────────────────────────────────────────────
        var invoices = await _uow.SalesInvoices.GetQueryable()
            .Include(i => i.Customer)
            .Where(i => !i.IsDeleted && i.InvoiceNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(i => i.InvoiceDate)
            .Take(MaxPerCategory)
            .ToListAsync();

        if (invoices.Any())
            results.Add(new GlobalSearchResultDto
            {
                Category = "Sales Invoices",
                Icon = "bi-receipt",
                Items = invoices.Select(i => new SearchResultItemDto
                {
                    Title = i.InvoiceNumber,
                    Subtitle = $"{(i.Customer != null ? i.Customer.Name : "Walk-in")} · {i.InvoiceDate:dd MMM yyyy}",
                    Url = $"/Sales/Details/{i.Id}",
                    Badge = $"${i.TotalAmount:N2}",
                    BadgeColor = i.DueAmount > 0 ? "warning" : "success"
                }).ToList()
            });

        // ── Suppliers ─────────────────────────────────────────────────────────
        var suppliers = await _uow.Suppliers.GetQueryable()
            .Where(s => !s.IsDeleted && s.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .Take(MaxPerCategory)
            .ToListAsync();

        if (suppliers.Any())
            results.Add(new GlobalSearchResultDto
            {
                Category = "Suppliers",
                Icon = "bi-truck",
                Items = suppliers.Select(s => new SearchResultItemDto
                {
                    Title = s.CompanyName,
                    Subtitle = s.ContactPerson ?? s.PhoneNumber,
                    Url = $"/Supplier/Details/{s.Id}",
                    Badge = s.CurrentBalance > 0 ? $"Payable ${s.CurrentBalance:N2}" : null,
                    BadgeColor = "warning"
                }).ToList()
            });

        // ── Purchase Orders ───────────────────────────────────────────────────
        var pos = await _uow.PurchaseOrders.GetQueryable()
            .Include(p => p.Supplier)
            .Where(p => !p.IsDeleted && p.PONumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.OrderDate)
            .Take(MaxPerCategory)
            .ToListAsync();

        if (pos.Any())
            results.Add(new GlobalSearchResultDto
            {
                Category = "Purchase Orders",
                Icon = "bi-bag-check",
                Items = pos.Select(p => new SearchResultItemDto
                {
                    Title = p.PONumber,
                    Subtitle = $"{p.Supplier.CompanyName} · {p.OrderDate:dd MMM yyyy}",
                    Url = $"/Purchase/Details/{p.Id}",
                    Badge = p.Status.ToString(),
                    BadgeColor = "info"
                }).ToList()
            });

        // ── Gift Cards (Feature #3 থেকে) ─────────────────────────────────────
        var giftCards = await _uow.GiftCards.GetQueryable()
            .Where(g => !g.IsDeleted && g.CardCode.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .Take(MaxPerCategory)
            .ToListAsync();

        if (giftCards.Any())
            results.Add(new GlobalSearchResultDto
            {
                Category = "Gift Cards",
                Icon = "bi-credit-card-2-front",
                Items = giftCards.Select(g => new SearchResultItemDto
                {
                    Title = g.CardCode,
                    Subtitle = g.IsStoreCredit ? "Store Credit" : "Gift Card",
                    Url = $"/GiftCard/Details/{g.Id}",
                    Badge = $"${g.CurrentBalance:N2}",
                    BadgeColor = g.Status == "Active" ? "success" : "secondary"
                }).ToList()
            });

        return results;
    }
}