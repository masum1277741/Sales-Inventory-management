namespace ClothingERP.Web.Controllers;

[AllowAnonymous]
public class ShopController : Controller
{
    private readonly IStorefrontService _storefrontSvc;
    private readonly IOnlineOrderService _orderSvc;
    private readonly IExchangeRateService _rateSvc;
    private readonly IPaymentGatewayService? _paymentSvc;   // Feature #20, optional

    public ShopController(IStorefrontService storefrontSvc, IOnlineOrderService orderSvc,
        IExchangeRateService rateSvc, IPaymentGatewayService? paymentSvc = null)
        => (_storefrontSvc, _orderSvc, _rateSvc, _paymentSvc) = (storefrontSvc, orderSvc, rateSvc, paymentSvc);

    // ── Homepage ──────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var settings = await _storefrontSvc.GetSettingsAsync();
        if (!settings.IsStoreEnabled) return View("StoreDisabled");

        ViewBag.Settings = settings;
        ViewBag.Rates = await _rateSvc.GetCurrentRatesAsync();
        var featured = await _storefrontSvc.GetFeaturedProductsAsync(8);
        return View(featured);
    }

    // ── Catalog/Category Browsing ─────────────────────────────────────────
    public async Task<IActionResult> Catalog(ProductFilterDto filter)
    {
        ViewBag.Settings = await _storefrontSvc.GetSettingsAsync();
        ViewBag.Rates = await _rateSvc.GetCurrentRatesAsync();
        var result = await _storefrontSvc.GetProductsAsync(filter);
        ViewBag.Filter = filter;
        return View(result);
    }

    // ── Product Detail ───────────────────────────────────────────────────
    public async Task<IActionResult> Product(int id)
    {
        var product = await _storefrontSvc.GetProductDetailAsync(id);
        if (product == null) return NotFound();
        ViewBag.Settings = await _storefrontSvc.GetSettingsAsync();
        ViewBag.Rates = await _rateSvc.GetCurrentRatesAsync();
        return View(product);
    }

    // ── Cart Page (localStorage থেকে JS render করবে, server শুধু layout দেয়) ──
    public async Task<IActionResult> Cart()
    {
        ViewBag.Settings = await _storefrontSvc.GetSettingsAsync();
        ViewBag.Rates = await _rateSvc.GetCurrentRatesAsync();
        return View();
    }

    // ── AJAX: Cart Pricing (cart page এ live update দেখানোর জন্য) ──────────
    [HttpPost]
    public async Task<IActionResult> PriceCart([FromBody] CartPricingRequestDto dto)
    {
        var result = await _orderSvc.PriceCartAsync(dto, "USD");
        return Json(result);
    }

    // ── Checkout Page ────────────────────────────────────────────────────
    public async Task<IActionResult> Checkout()
    {
        ViewBag.Settings = await _storefrontSvc.GetSettingsAsync();
        ViewBag.Rates = await _rateSvc.GetCurrentRatesAsync();
        ViewBag.IsLoggedIn = User.Identity?.IsAuthenticated == true;
        return View();
    }

    // ── AJAX: Place Order ─────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutDto dto)
    {
        int? customerId = null;
        if (User.Identity?.IsAuthenticated == true)
            customerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var result = await _orderSvc.CheckoutAsync(dto, customerId, customerId);
        if (!result.Success) return Json(new { success = false, message = result.Message });

        // ── bKash/Nagad হলে payment initiate করো (Feature #20) ────────────────
        if (dto.PaymentMethod is "bKash" or "Nagad" && _paymentSvc != null)
        {
            var payResult = await _paymentSvc.InitiatePaymentAsync(new InitiatePaymentDto
            {
                Provider = dto.PaymentMethod,
                AmountUSD = result.Data!.TotalUSD,
                CustomerMsisdn = dto.Phone
            }, customerId ?? 0);

            result.Data.DigitalPaymentRedirectUrl = payResult.RedirectUrl;
        }

        return Json(new { success = true, message = result.Message, data = result.Data });
    }

    // ── Order Confirmation Page ────────────────────────────────────────────
    public IActionResult OrderConfirmation(string orderNumber) => View((object)orderNumber);
}