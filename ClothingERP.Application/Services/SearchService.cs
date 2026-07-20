using Microsoft.EntityFrameworkCore;
using ClothingERP.Application.DTOs;
using ClothingERP.Application.Interfaces;
using ClothingERP.Application.Interfaces.Services;

namespace ClothingERP.Application.Services;

public class SearchService : ISearchService
{
    private readonly IUnitOfWork _uow;
    private const int Max = 5;

    public SearchService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<GlobalSearchResultDto>> GlobalSearchAsync(string keyword, int currentUserId)
    {
        var results = new List<GlobalSearchResultDto>();
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 2)
            return results;

        var kw = keyword.Trim().ToLower();

        // ── Products / Variants ──────────────────────────────────────────
        try
        {
            var variants = await _uow.ProductVariants.GetQueryable()
                .Include(v => v.Product)
                .Include(v => v.Size)
                .Include(v => v.Color)
                .Include(v => v.Stock)
                .Where(v => !v.IsDeleted && v.IsActive && !v.Product.IsDeleted &&
                           (v.Product.Name.ToLower().Contains(kw) ||
                            v.Barcode.ToLower().Contains(kw)))
                .Take(Max)
                .ToListAsync();

            if (variants.Any())
                results.Add(new GlobalSearchResultDto
                {
                    Category = "Products",
                    Icon = "bi-box-seam",
                    Items = variants.Select(v => new SearchResultItemDto
                    {
                        Title = v.Product.Name,
                        Subtitle = $"{v.Size?.Name} · {v.Color?.Name} · {v.Barcode}",
                        Url = $"/Product/Details/{v.ProductId}",
                        Badge = (v.Stock?.Quantity ?? 0) > 0
                                     ? $"{v.Stock!.Quantity} in stock"
                                     : "Out of stock",
                        BadgeColor = (v.Stock?.Quantity ?? 0) > 0 ? "success" : "danger"
                    }).ToList()
                });
        }
        catch (Exception ex)
        {

            Console.WriteLine($"[Search] Products error: {ex.Message}");
        }

        // ── Customers ────────────────────────────────────────────────────
        try
        {
            var customers = await _uow.Customers.GetQueryable()
                .Where(c => !c.IsDeleted && c.IsActive &&
                           (c.Name.ToLower().Contains(kw) ||
                           (c.PhoneNumber != null && c.PhoneNumber.Contains(kw))))
                .Take(Max)
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
                        Badge = c.CurrentBalance > 0
                                     ? $"Due ${c.CurrentBalance:N2}"
                                     : "No due",
                        BadgeColor = c.CurrentBalance > 0 ? "danger" : "success"
                    }).ToList()
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Search] Customers error: {ex.Message}");
        }

        // ── Sales Invoices ────────────────────────────────────────────────
        try
        {
            var invoices = await _uow.SalesInvoices.GetQueryable()
                .Include(i => i.Customer)
                .Where(i => !i.IsDeleted && i.InvoiceNumber.ToLower().Contains(kw))
                .OrderByDescending(i => i.InvoiceDate)
                .Take(Max)
                .ToListAsync();

            if (invoices.Any())
                results.Add(new GlobalSearchResultDto
                {
                    Category = "Invoices",
                    Icon = "bi-receipt",
                    Items = invoices.Select(i => new SearchResultItemDto
                    {
                        Title = i.InvoiceNumber,
                        Subtitle = $"{i.Customer?.Name ?? "Walk-in"} · {i.InvoiceDate:dd MMM yyyy}",
                        Url = $"/Sales/Details/{i.Id}",
                        Badge = $"${i.TotalAmount:N2}",
                        BadgeColor = i.DueAmount > 0 ? "warning" : "success"
                    }).ToList()
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Search] Invoices error: {ex.Message}");
        }

        // ── Suppliers ─────────────────────────────────────────────────────
        try
        {
            var suppliers = await _uow.Suppliers.GetQueryable()
                .Where(s => !s.IsDeleted && s.CompanyName.ToLower().Contains(kw))
                .Take(Max)
                .ToListAsync();

            if (suppliers.Any())
                results.Add(new GlobalSearchResultDto
                {
                    Category = "Suppliers",
                    Icon = "bi-truck",
                    Items = suppliers.Select(s => new SearchResultItemDto
                    {
                        Title = s.CompanyName,
                        Subtitle = s.ContactPerson ?? s.PhoneNumber ?? "",
                        Url = $"/Supplier/Details/{s.Id}",
                        Badge = s.CurrentBalance > 0
                                     ? $"Payable ${s.CurrentBalance:N2}"
                                     : null,
                        BadgeColor = "warning"
                    }).ToList()
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Search] Suppliers error: {ex.Message}");
        }

        return results;
    }
}