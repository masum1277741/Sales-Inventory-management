namespace ClothingERP.Web.Controllers;

public class GiftCardController : BaseController
{
    private readonly IGiftCardService _giftCardSvc;
    private readonly ICustomerService _customerSvc;

    public GiftCardController(IGiftCardService giftCardSvc, ICustomerService customerSvc)
        => (_giftCardSvc, _customerSvc) = (giftCardSvc, customerSvc);

    // ── Index ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Gift Cards & Store Credit";
        var cards = (await _giftCardSvc.GetAllAsync()).ToList();

        ViewBag.TotalActive = cards.Count(c => c.Status == "Active");
        ViewBag.TotalOutstanding = cards.Where(c => c.Status == "Active").Sum(c => c.CurrentBalance);
        ViewBag.TotalGiftCards = cards.Count(c => !c.IsStoreCredit);
        ViewBag.TotalStoreCredits = cards.Count(c => c.IsStoreCredit);

        return View(cards);
    }

    // ── Issue Gift Card (purchased) ──────────────────────────────────────
    [HttpGet]
    public IActionResult Issue()
    {
        ViewData["Title"] = "Issue Gift Card";
        return View(new IssueGiftCardDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Issue(IssueGiftCardDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _giftCardSvc.IssueAsync(dto, CurrentUserId);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }

        TempData["success"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    // ── Issue Store Credit (manual / from return) ────────────────────────
    [HttpGet]
    public IActionResult IssueStoreCredit(int? customerId)
    {
        ViewData["Title"] = "Issue Store Credit";
        return View(new IssueStoreCreditDto { CustomerId = customerId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> IssueStoreCredit(IssueStoreCreditDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _giftCardSvc.IssueStoreCreditAsync(dto, CurrentUserId);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }

        TempData["success"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    // ── Details ───────────────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Gift Card Details";
        var card = await _giftCardSvc.GetByIdAsync(id);
        if (card == null) return NotFound();
        return View(card);
    }

    // ── AJAX: Lookup (POS এর জন্য) ─────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Lookup(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return Json(new GiftCardLookupDto { Found = false });
        var result = await _giftCardSvc.LookupAsync(code);
        return Json(result);
    }

    // ── AJAX: Cancel ──────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var r = await _giftCardSvc.CancelAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── AJAX: Customer Search (Issue ফর্মের জন্য) ─────────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchCustomers(string keyword)
    {
        var list = (await _customerSvc.GetAllAsync())
            .Where(c => c.IsActive && (string.IsNullOrEmpty(keyword) ||
                        c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .Take(15)
            .Select(c => new { c.Id, c.Name, c.PhoneNumber });
        return Json(list);
    }
}