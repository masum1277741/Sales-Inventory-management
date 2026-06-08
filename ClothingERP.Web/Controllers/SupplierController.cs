namespace ClothingERP.Web.Controllers;

public class SupplierController : BaseController
{
    private readonly ISupplierService _supplierSvc;

    public SupplierController(ISupplierService supplierSvc) => _supplierSvc = supplierSvc;

    // ── Index ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Supplier Management";
        return View(await _supplierSvc.GetAllAsync());
    }

    // ── Create ────────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Add Supplier";
        return View(new CreateSupplierDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSupplierDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _supplierSvc.CreateAsync(dto, CurrentUserId);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ──────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Supplier";
        var supplier = await _supplierSvc.GetByIdAsync(id);
        if (supplier == null) return NotFound();
        ViewBag.Supplier = supplier;
        return View(new UpdateSupplierDto
        {
            CompanyName = supplier.CompanyName,
            ContactPerson = supplier.ContactPerson,
            PhoneNumber = supplier.PhoneNumber,
            Email = supplier.Email,
            Address = supplier.Address,
            BankName = supplier.BankName,
            BankAccountNumber = supplier.BankAccountNumber,
            IsActive = supplier.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateSupplierDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _supplierSvc.UpdateAsync(id, dto, CurrentUserId);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── Ledger ────────────────────────────────────────────────────────────
    public async Task<IActionResult> Ledger(int id, DateTime? from = null, DateTime? to = null)
    {
        ViewData["Title"] = "Supplier Ledger";
        var supplier = await _supplierSvc.GetByIdAsync(id);
        if (supplier == null) return NotFound();

        var fromDate = from ?? DateTime.Today.AddDays(-90);
        var toDate = to ?? DateTime.Today;
        var ledger = await _supplierSvc.GetLedgerAsync(id, fromDate, toDate.AddDays(1));
        var balance = await _supplierSvc.GetBalanceAsync(id);

        ViewBag.Supplier = supplier;
        ViewBag.Balance = balance;
        ViewBag.FromDate = fromDate.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate.ToString("yyyy-MM-dd");
        ViewBag.TotalDebit = ledger.Sum(l => l.Debit);
        ViewBag.TotalCredit = ledger.Sum(l => l.Credit);

        return View(ledger);
    }

    // ── AJAX ─────────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _supplierSvc.DeleteAsync(id);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var r = await _supplierSvc.ToggleStatusAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPayment(int supplierId, decimal amount, int method, string? reference)
    {
        if (!Enum.IsDefined(typeof(PaymentMethod), method)) method = 1;
        var r = await _supplierSvc.AddPaymentAsync(supplierId, amount, (PaymentMethod)method, reference, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return Json(Array.Empty<object>());
        return Json((await _supplierSvc.GetAllAsync())
            .Where(s => s.IsActive &&
                       (s.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        s.PhoneNumber.Contains(keyword)))
            .Take(10)
            .Select(s => new { s.Id, s.CompanyName, s.PhoneNumber, s.CurrentBalance }));
    }
}