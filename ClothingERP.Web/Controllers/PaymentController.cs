namespace ClothingERP.Web.Controllers;

public class PaymentController : BaseController
{
    private readonly IPaymentGatewayService _paymentSvc;

    public PaymentController(IPaymentGatewayService paymentSvc) => _paymentSvc = paymentSvc;

    // ── AJAX: POS থেকে Payment শুরু করো ─────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Initiate(InitiatePaymentDto dto)
    {
        var result = await _paymentSvc.InitiatePaymentAsync(dto, CurrentUserId);
        return Json(result);
    }

    // ── AJAX: POS থেকে Polling করে Status চেক ──────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Status(string paymentId)
    {
        var status = await _paymentSvc.CheckStatusAsync(paymentId);
        return Json(status);
    }

    // ── bKash Callback (browser redirect — AllowAnonymous প্রয়োজন) ────────
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> BkashCallback(string paymentID, string status)
    {
        var result = await _paymentSvc.HandleCallbackAsync("bKash", paymentID, status);
        ViewBag.Success = result.Success;
        ViewBag.Message = result.Message;
        return View("CallbackResult");
    }

    // ── Nagad Callback ───────────────────────────────────────────────────────
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> NagadCallback(string payment_ref_id, string status)
    {
        var result = await _paymentSvc.HandleCallbackAsync("Nagad", payment_ref_id, status);
        ViewBag.Success = result.Success;
        ViewBag.Message = result.Message;
        return View("CallbackResult");
    }

    // ── History পেইজ (admin দেখার জন্য) ───────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Digital Payment Transactions";
        var txns = await _paymentSvc.GetRecentTransactionsAsync();
        return View(txns);
    }
}