namespace ClothingERP.Web.Controllers;

public class CustomerController : BaseController
{
    private readonly ICustomerService _customerSvc;
    private readonly IWebHostEnvironment _env;

    public CustomerController(ICustomerService customerSvc, IWebHostEnvironment env)
        => (_customerSvc, _env) = (customerSvc, env);

    // ── Index ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Customer Management";
        return View(await _customerSvc.GetAllAsync());
    }

    // ── Create ────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Add Customer";
        await LoadGroups();
        return View(new CreateCustomerDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerDto dto, IFormFile? profileImage)
    {
        if (profileImage is { Length: > 0 })
            dto.ProfileImagePath = await SaveFile(profileImage, "customers");

        if (!ModelState.IsValid) { await LoadGroups(); return View(dto); }

        var result = await _customerSvc.CreateAsync(dto, CurrentUserId);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message!);
            await LoadGroups();
            return View(dto);
        }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ──────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Customer";
        var customer = await _customerSvc.GetByIdAsync(id);
        if (customer == null) return NotFound();

        ViewBag.Customer = customer;
        await LoadGroups();
        return View(new UpdateCustomerDto
        {
            Name = customer.Name,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Address = customer.Address,
            NIDNumber = customer.NIDNumber,
            CustomerGroupId = customer.CustomerGroupId,
            IsActive = customer.IsActive,
            ProfileImagePath = customer.ProfileImage
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCustomerDto dto, IFormFile? profileImage)
    {
        if (profileImage is { Length: > 0 })
            dto.ProfileImagePath = await SaveFile(profileImage, "customers");

        if (!ModelState.IsValid) { await LoadGroups(); return View(dto); }

        var result = await _customerSvc.UpdateAsync(id, dto, CurrentUserId);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message!);
            await LoadGroups();
            return View(dto);
        }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── Ledger ────────────────────────────────────────────────────────────
    public async Task<IActionResult> Ledger(int id, DateTime? from = null, DateTime? to = null)
    {
        ViewData["Title"] = "Customer Ledger";
        var customer = await _customerSvc.GetByIdAsync(id);
        if (customer == null) return NotFound();

        var fromDate = from ?? DateTime.Today.AddDays(-90);
        var toDate = to ?? DateTime.Today;
        var ledger = await _customerSvc.GetLedgerAsync(id, fromDate, toDate.AddDays(1));
        var balance = await _customerSvc.GetBalanceAsync(id);

        ViewBag.Customer = customer;
        ViewBag.Balance = balance;
        ViewBag.FromDate = fromDate.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate.ToString("yyyy-MM-dd");
        ViewBag.TotalDebit = ledger.Sum(l => l.Debit);
        ViewBag.TotalCredit = ledger.Sum(l => l.Credit);

        return View(ledger);
    }

    // ── Groups ────────────────────────────────────────────────────────────
    public async Task<IActionResult> Groups()
    {
        ViewData["Title"] = "Customer Groups";
        return View(await _customerSvc.GetGroupsAsync());
    }

    // ── AJAX Actions ──────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _customerSvc.DeleteAsync(id);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var r = await _customerSvc.ToggleStatusAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPayment(int customerId, decimal amount,
        string description, string? reference)
    {
        var r = await _customerSvc.AddPaymentAsync(customerId, amount, description, reference, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGroup(CreateCustomerGroupDto dto)
    {
        var r = await _customerSvc.CreateGroupAsync(dto, CurrentUserId);
        return r.Success ? JsonSuccess(r.Data, r.Message!) : JsonError(r.Message!);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? keyword)
    {
        var all = (await _customerSvc.GetAllAsync())
            .Where(c => c.IsActive)
            .ToList();

   
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Json(all
                .Take(50)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name ?? "",
                    phoneNumber = c.PhoneNumber ?? "",
                    currentBalance = c.CurrentBalance,
                    groupName = c.GroupName ?? ""
                }));
        }

      
        return Json(all
            .Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        (c.PhoneNumber != null &&
                         c.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .Take(20)
            .Select(c => new
            {
                id = c.Id,
                name = c.Name ?? "",
                phoneNumber = c.PhoneNumber ?? "",
                currentBalance = c.CurrentBalance,
                groupName = c.GroupName ?? ""
            }));
    }

    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _customerSvc.GetByIdAsync(id);
        return c == null ? NotFound() : Json(c);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private async Task LoadGroups()
        => ViewBag.Groups = await _customerSvc.GetGroupsAsync();

    private async Task<string> SaveFile(IFormFile file, string folder)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(dir);
        var name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var s = new FileStream(Path.Combine(dir, name), FileMode.Create);
        await file.CopyToAsync(s);
        return $"/uploads/{folder}/{name}";
    }
}