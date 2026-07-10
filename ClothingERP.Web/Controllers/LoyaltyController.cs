namespace ClothingERP.Web.Controllers;

public class LoyaltyController : BaseController
{
    private readonly ILoyaltyService _loyaltySvc;

    public LoyaltyController(ILoyaltyService loyaltySvc) => _loyaltySvc = loyaltySvc;

    // ── Settings ──────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        ViewData["Title"] = "Loyalty Program Settings";
        var settings = await _loyaltySvc.GetSettingsAsync();
        return View(settings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(UpdateLoyaltySettingsDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Loyalty Program Settings";
            return View(dto);
        }

        var result = await _loyaltySvc.UpdateSettingsAsync(dto, CurrentUserId);
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Settings));
    }

    // ── AJAX: Customer Loyalty Info (POS থেকে call হবে) ──────────────────
    [HttpGet]
    public async Task<IActionResult> GetCustomerLoyalty(int customerId)
    {
        var info = await _loyaltySvc.GetCustomerLoyaltyAsync(customerId);
        return Json(info);
    }

    // ── AJAX: Redeem Preview ──────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> PreviewRedeem(int customerId, int points)
    {
        var preview = await _loyaltySvc.PreviewRedeemAsync(customerId, points);
        return Json(preview);
    }

    // ── Manual Birthday Bonus Trigger (admin only) ───────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RunBirthdayBonus()
    {
        var count = await _loyaltySvc.ApplyBirthdayBonusesAsync(CurrentUserId);
        return JsonSuccess(message: $"{count} customer(s) received birthday bonus points.");
    }
}