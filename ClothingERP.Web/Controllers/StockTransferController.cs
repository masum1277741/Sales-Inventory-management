namespace ClothingERP.Web.Controllers;

public class StockTransferController : BaseController
{
    private readonly IStockTransferService _transferSvc;
    private readonly IBranchService _branchSvc;
    private readonly ICurrentBranchProvider _currentBranch;
    private readonly IProductService _productSvc;

    public StockTransferController(IStockTransferService transferSvc, IBranchService branchSvc,
        ICurrentBranchProvider currentBranch, IProductService productSvc)
        => (_transferSvc, _branchSvc, _currentBranch, _productSvc) = (transferSvc, branchSvc, currentBranch, productSvc);

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Stock Transfers";
        var transfers = await _transferSvc.GetAllAsync(_currentBranch.GetCurrentBranchId());
        return View(transfers);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "New Stock Transfer";
        ViewBag.Branches = (await _branchSvc.GetAllAsync()).Where(b => b.Id != _currentBranch.GetCurrentBranchId());
        ViewBag.FromBranchId = _currentBranch.GetCurrentBranchId();
        return View(new CreateStockTransferDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStockTransferDto dto)
    {
        var result = await _transferSvc.CreateAsync(dto, _currentBranch.GetCurrentBranchId(), CurrentUserId);
        if (!result.Success) { TempData["error"] = result.Message; return RedirectToAction(nameof(Create)); }
        TempData["success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Transfer Details";
        var transfer = await _transferSvc.GetByIdAsync(id);
        if (transfer == null) return NotFound();
        return View(transfer);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive([FromBody] ReceiveStockTransferDto dto)
    {
        var r = await _transferSvc.ReceiveAsync(dto, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var r = await _transferSvc.CancelAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpGet]
    public async Task<IActionResult> SearchVariants(string keyword)
    {
        var variants = await _productSvc.SearchVariantsAsync(keyword);
        return Json(variants.Take(15));
    }
}