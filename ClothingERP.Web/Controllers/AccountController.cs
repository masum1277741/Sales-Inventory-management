namespace ClothingERP.Web.Controllers;

public class AccountController : BaseController
{
    private readonly IAccountService _accountSvc;

    public AccountController(IAccountService accountSvc) => _accountSvc = accountSvc;

    // ── Index ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(DateTime? fromDate = null, DateTime? toDate = null,
        string? type = null)
    {
        ViewData["Title"] = "Account Transactions";
        var all = (await _accountSvc.GetAllAsync()).ToList();
        var from = fromDate ?? DateTime.Today.AddMonths(-1);
        var to = toDate ?? DateTime.Today;

        var filtered = all.Where(t => t.TransactionDate.Date >= from &&
                                      t.TransactionDate.Date <= to);
        if (!string.IsNullOrEmpty(type))
            filtered = filtered.Where(t => t.TransactionType == type);

        var filteredList = filtered.OrderByDescending(t => t.TransactionDate).ToList();

        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        ViewBag.FilterType = type;
        ViewBag.TotalIncome = all.Where(t => t.TransactionType == "Income").Sum(t => t.Amount);
        ViewBag.TotalExpense = all.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount);
        ViewBag.NetBalance = (decimal)ViewBag.TotalIncome - (decimal)ViewBag.TotalExpense;

        return View(filteredList);
    }

    // ── Create ────────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Add Transaction";
        return View(new CreateAccountTransactionDto { TransactionDate = DateTime.Today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccountTransactionDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _accountSvc.CreateAsync(dto, CurrentUserId);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── AJAX: Delete ──────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _accountSvc.DeleteAsync(id);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }
}